using Approval.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PureCode.Managers;
using PureCode.Utils;
using SeafileClient;
using SeafileClient.Common;
using SeafileClient.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Managers
{
  public class SeafileFileInfo
  {
    public string Title { get; set; }
    public string RepoId { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string RepoName { get; set; }
    public string FileId { get; internal set; }
  }
  public class SeafileManager
  {
    private readonly SettingManager settingManager;
    private readonly ISeafileApi api;
    private readonly SeafileOptions options;
    private string token;
    public SeafileManager(SettingManager settingManager, IOptions<SeafileOptions> optionsAccessor, ISeafileApi api)
    {
      this.settingManager = settingManager;
      this.api = api;
      this.options = optionsAccessor.Value;
    }

    public async Task<string> GetApprovalAccountTokenAsync()
    {
      if (string.IsNullOrEmpty(token))
      {
        var setting = await settingManager.GetGlobalSettings<SeafileAccounts>();
        if (setting.Values != null && setting.Values.TryGetValue(options.ApprovalUserName, out var approvalToken))
        {
          token = approvalToken;

        }
        else
        {
          token = api.Login(options.ApprovalUserName, options.ApprovalPassword);
          setting.Values[options.ApprovalUserName] = token;
          await settingManager.SaveGlobalSettingAsync(setting);
        }
      }
      return token;
    }

    public async Task<DownloadFileInfo> GetFileEditorUrlAsync(string repoId, string filePath)
    {
      var token = await GetApprovalAccountTokenAsync();
      var info = api.GetDownloadLink(token, repoId, filePath, true, true);
      return info;
    }

    public async Task<Repo> GetRepoAsync(string templateName)
    {
      var token = await GetApprovalAccountTokenAsync();
      var repos = api.ListRepos(token);
      var repo = repos.FirstOrDefault(x => x.Name == templateName);
      if (repo == null)
      {
        var newRepo = api.CreateRepo(token, templateName);
        PrepareFolders(token, new string[] { newRepo.Name });
        return new Repo
        {
          Id = newRepo.Id,
          Name = newRepo.Name
        };
      }
      return repo;
    }

    private void PrepareFolders(string token, IEnumerable<string> templateNames)
    {
      var repos = api.ListRepos(token);
      var all = new List<Repo>();
      foreach (var name in templateNames)
      {
        var repo = repos.FirstOrDefault(x => x.Name == name);
        if (repo == null)
        {
          var newRepo = api.CreateRepo(token, name);
          all.Add(new Repo { Name = newRepo.Name, Id = newRepo.Id });
        }
        else
        {
          all.Add(repo);
        }
      }
      var today = $"{DateTime.Now:yyyMMdd}";
      var dates = Enumerable.Range(0, 30).Select(offset => DateTime.Now.AddDays(offset)).Select(x => $"{x:yyyMMdd}");
      foreach (var repo in all)
      {
        var dirs = api.ListDirectories(token, repo.Id);
        if (!dirs.Any(x => x.Name == today))
        {
          dates.ForEach(x => api.CreateDirectory(token, repo.Id, $"/{x}"));
        }
      }
    }

    private async Task PrepareFoldersAsync(IEnumerable<string> templateNames)
    {
      var token = await GetApprovalAccountTokenAsync();

      PrepareFolders(token, templateNames);
    }

    public async Task<SeafileAttach> UploadFileAsync(string filePath, string templateName, int itemId)
    {
      var token = await GetApprovalAccountTokenAsync();
      var repo = await GetRepoAsync(templateName);
      var dir = $"/{DateTime.Now:yyyMMdd}/{itemId}";
      IEnumerable<DirEntry> entries;
      try
      {
        entries = api.ListDirectories(token, repo.Id, $"/{DateTime.Now:yyyMMdd}");
      } 
      catch
      {
        api.CreateDirectory(token, repo.Id, $"/{DateTime.Now:yyyMMdd}");
        entries = new List<DirEntry>();
      }
      if(!entries.Any(x => x.Name == itemId.ToString()))
      {
        api.CreateDirectory(token, repo.Id, dir);
      }

      var link = api.CreateUploadLink(token, repo.Id, dir);

      var result = api.UploadFiles(token, link, new string[] { filePath }, dir);
      if (result != null && result.Any())
      {
        var first = result.First();
        var path = $"/{DateTime.Now:yyyMMdd}/{itemId}/{first.Name}";
        var id = HashUtils.GenerateMd5(path);
        return new SeafileAttach
        {
          Url = $"/seafile/approval/{itemId}/{id}",
          RepoId = repo.Id,
          RepoName = repo.Name,
          FilePath = path,
          FileName = first.Name,
          FileId = id
        };
      }
      return null;
    }

    public async Task MoveFileAsync(string templateName, string sourcePath, string targetPath)
    {
      var token = await GetApprovalAccountTokenAsync();
      var repo = await GetRepoAsync(templateName);
      try
      {
        api.ListDirectories(token, repo.Id, targetPath).Any();
      }
      catch(SeafileException e)
      {
        if (e.Message.EndsWith("not found.\"}"))
        {
          api.CreateDirectory(token, repo.Id, targetPath);
        }
      }
      try
      {
        api.MoveFile(token, repo.Id, sourcePath, repo.Id, targetPath);
      } catch(SeafileException e)
      {

      }
    }
  }
}
