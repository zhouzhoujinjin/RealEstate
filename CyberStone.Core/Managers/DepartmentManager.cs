using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberStone.Core.Utils;

namespace CyberStone.Core.Managers
{
  public class DepartmentManager
  {
    private readonly CyberStoneDbContext _context;
    private readonly UserManager _userManager;

    public DepartmentManager(CyberStoneDbContext context, UserManager userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    public async Task<IEnumerable<UserDepartment>> GetUserDepartmentsAsync(long userId)
    {
      var departments = await _context.DepartmentUsers
        .Include(e => e.Department)
        .Where(e => e.UserId == userId)
        .Select(x => new UserDepartment
        {
          DepartmentId = x.DepartmentId,
          Title = x.Department!.Title,
          IsUserMajorDepartment = x.IsUserMajorDepartment,
          Level = x.Level,
          Position = x.Position
        })
        .ToListAsync();
      return departments;
    }

    public async Task<UserDepartment?> GetUserMajorDepartmentAsync(long userId)
    {
      var department = await _context.DepartmentUsers
        .Include(e => e.Department)
        .Where(e => e.UserId == userId && e.IsUserMajorDepartment)
        .Select(x => new UserDepartment
        {
          DepartmentId = x.DepartmentId,
          Title = x.Department!.Title,
          IsUserMajorDepartment = x.IsUserMajorDepartment,
          Level = x.Level,
          Position = x.Position
        })
        .FirstOrDefaultAsync();
      return department;
    }

    public async Task<IEnumerable<DepartmentUser>> GetDepartmentUsersAsync(long departmentId,
      bool withSubDepartments = false)
    {
      List<DepartmentUser> departmentUsers;
      if (!withSubDepartments)
      {
        departmentUsers = await _context.DepartmentUsers
          .Where(e => e.DepartmentId == departmentId)
          .Select(x => new DepartmentUser
          {
            Id = x.UserId,
            Level = x.Level,
            Position = x.Position,
            IsUserMajorDepartment = x.IsUserMajorDepartment
          }).ToListAsync();
        await departmentUsers.ForEachAsync(async x =>
        {
          var user = await _userManager.GetBriefUserAsync(x.Id);
          x.UserName = user!.UserName;
          x.Profiles = user!.Profiles;
        });
      }
      else
      {
        var root = await GetDepartmentTreeAsync(departmentId);
        var departmentIds = new List<long>();
        GetChildIds(root, ref departmentIds);

        departmentUsers = await _context.DepartmentUsers
          .Where(e => departmentIds.Contains(e.DepartmentId))
          .Select(x => new DepartmentUser
          {
            Id = x.UserId,
            Level = x.Level,
            Position = x.Position,
            IsUserMajorDepartment = x.IsUserMajorDepartment
          }).ToListAsync();
        await departmentUsers.ForEachAsync(async x =>
        {
          var user = await _userManager.GetBriefUserAsync(x.Id);
          x.UserName = user!.UserName;
          x.Profiles = user!.Profiles;
        });
      }

      return departmentUsers;
    }

    public async Task<Department?> GetDepartmentWithUsersAsync(long departmentId)
    {
      var entity = await _context.Departments.Include(x => x.Users).Where(e => e.Id == departmentId)
        .FirstOrDefaultAsync();
      if (entity == null)
      {
        return null;
      }

      var department = await EntityToModelAsync(entity, async (e, m) =>
      {
        var users = new List<DepartmentUser>();
        foreach (var x in e.Users)
        {
          var u = await _userManager.GetBriefUserAsync(x.UserId);
          var du = DepartmentUser.FromUser(u!);
          du.IsUserMajorDepartment = x.IsUserMajorDepartment;
          du.Level = x.Level;
          du.Position = x.Position;
          users.Add(du);
        }
        m.Users = users;
        return m;
      });
      return department;
    }

    public async Task<DepartmentEntity> GetDepartmentAsync(long? departmentId, bool withUsers = false,
      bool includeChildren = false)
    {
      DepartmentEntity? root = null;
      if (includeChildren)
      {
        var departments = await _context.Departments.ToListAsync();
        foreach (var d in departments)
        {
          if (departmentId != null && d.Id == departmentId)
          {
            root = d;
          }
          else if (departmentId == null && d.Parent == null)
          {
            root = d;
          }
        }

        if (withUsers && root != null)
        {
          root.Users = await _context.DepartmentUsers.Where(x => x.DepartmentId == root.Id).ToListAsync();
        }
      }
      else
      {
        departmentId ??= 1;
        var query = _context.Departments.Where(x => x.Id == departmentId!);
        if (withUsers)
        {
          root = await query.Include(x => x.Users).FirstAsync();
        }
        else
        {
          root = await query.FirstAsync();
        }
      }

      return root!;
    }

    private void GetChildIds(Department department, ref List<long> ids)
    {
      ids.Add(department.Id);
      if (department.Children != null && !department.Children.Any()) return;
      foreach (var sub in department.Children!)
      {
        GetChildIds(sub, ref ids);
      }
    }

    public async Task<DepartmentEntity?> UpdateDepartmentAsync(long departmentId, Department department)
    {
      var entity = await _context.Departments.Include(x => x.Users).Where(x => x.Id == departmentId).FirstAsync();
      if (!string.IsNullOrEmpty(department.Title))
      {
        entity.Title = department.Title;
      }
      entity.Users = department.Users!.Select(x => new DepartmentUserEntity
      {
        DepartmentId = departmentId,
        UserId = x.Id,
        IsUserMajorDepartment = x.IsUserMajorDepartment ?? false,
        Level = x.Level ?? 0,
        Position = x.Position
      }).ToList();
      _context.Update(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
      var departments = await _context.Departments.ToListAsync();
      var models = new List<Department>();
      foreach (var d in departments)
      {
        models.Add(await EntityToModelAsync(d));
      }
      return models;
    }

    public async Task<Department> GetDepartmentTreeAsync(long? departmentId = null)
    {
      var departments = await _context.Departments.ToListAsync();
      var root = default(DepartmentEntity);
      foreach (var d in departments)
      {
        if (departmentId != null && d.Id == departmentId)
        {
          root = d;
        }
        else if (departmentId == null && d.Parent == null)
        {
          root = d;
        }
      }
      if (root == null) return null;
      var department = await EntityToModelAsync(root!);
      return department;
    }

    public async Task<Department?> CreateAsync(string title, long creatorId, long? parentId)
    {
      var entity = new DepartmentEntity
      {
        Title = title,
        ParentId = parentId,
        CreatorId = creatorId
      };
      _context.Add(entity);
      var result = await _context.SaveChangesAsync();
      if (result == 0)
      {
        return null;
      }

      var department = new Department
      {
        Id = entity.Id,
        Title = entity.Title
      };
      return department;
    }

    public async Task<Department?> UpdateAsync(int id, string title, int? parentId)
    {
      var entity = _context.Departments.FirstOrDefault(d => d.Id == id);

      if (entity == null)
      {
        return null;
      }

      entity.Title = title;
      entity.ParentId = parentId;

      _context.Update(entity);

      await _context.SaveChangesAsync();

      var department = new Department
      {
        Id = entity.Id,
        Title = entity.Title
      };
      return department;
    }

    public async Task DeleteAsync(long departmentId, bool includeChildren = false)
    {
      var departments = await _context.Departments.ToDictionaryAsync(e => e.Id, e => e);

      var deleteDepartments = new List<DepartmentEntity> { departments[departmentId] };
      var eventDepartments = new List<long>() { departmentId };
      if (includeChildren)
      {
        foreach (var kv in departments)
        {
          if (kv.Value.ParentId != null)
          {
            departments[kv.Value.ParentId.Value].Children.Add(kv.Value);
          }
        }

        var children = new Queue<DepartmentEntity>(departments[departmentId].Children);

        while (children.Count > 0)
        {
          deleteDepartments.AddRange(children);
          eventDepartments.AddRange(children.Select(e => e.Id));
          var newChildren = new Queue<DepartmentEntity>();
          while (children.Count > 0)
          {
            var child = children.Dequeue();
            child.Children.ForEach(c => newChildren.Enqueue(c));
          }

          children = newChildren;
        }
      }

      var users = _context.DepartmentUsers.Where(e => deleteDepartments.Select(d => d.Id).Contains(e.DepartmentId));
      _context.RemoveRange(users);
      _context.RemoveRange(deleteDepartments);
      await _context.SaveChangesAsync();
      eventDepartments.Reverse(); // 反向保证是从子部门开始删，这样不会有子节点，可以一层层的删除（企业微信的需求）
    }

    public async Task AddDepartmentUsers(long departmentId, IEnumerable<DepartmentUser> users)
    {
      var userIds = users.Select(u => u.Id);
      var query = _context.DepartmentUsers.Where(e => e.DepartmentId == departmentId && userIds.Contains(e.UserId));
      var existUsers = await query.ToListAsync();
      foreach (var user in users)
      {
        var existUser = existUsers.FirstOrDefault(u => u.UserId == user.Id);

        var userExistsDepartments = await _context.DepartmentUsers.Where(x => x.UserId == user.Id).CountAsync();
        if (existUser == null)
        {
          var u = new DepartmentUserEntity
          {
            DepartmentId = departmentId,
            UserId = user.Id,
            IsUserMajorDepartment = userExistsDepartments == 0 || (user.IsUserMajorDepartment ?? false),
            Position = user.Position,
            Level = user.Level ?? 0
          };
          _context.DepartmentUsers.Add(u);
          existUsers.Add(u);
        }
        else
        {
          existUser.IsUserMajorDepartment = userExistsDepartments == 0 || (user.IsUserMajorDepartment ?? false);
          existUser.Level = user.Level ?? 0;
          existUser.Position = user.Position;
        }
      }

      await _context.SaveChangesAsync();
    }

    public async Task SetUserDepartments(long userId, IEnumerable<UserDepartment> userDepartments)
    {
      var departments = await _context.Departments.ToArrayAsync();
      var departmentUsers = _context.DepartmentUsers.Where(du => du.UserId == userId)
        .ToDictionary(du => (du.DepartmentId, du.UserId), du => du);
      var remainMap = departmentUsers.ToDictionary(x => x.Key, _ => false);
      foreach (var u in userDepartments)
      {
        if (departmentUsers.ContainsKey((u.DepartmentId, userId)))
        {
          departmentUsers[(u.DepartmentId, userId)].Position = u.Position ?? departmentUsers[(u.DepartmentId, userId)].Position;
          departmentUsers[(u.DepartmentId, userId)].Level = u.Level ?? departmentUsers[(u.DepartmentId, userId)].Level;
          departmentUsers[(u.DepartmentId, userId)].IsUserMajorDepartment =
            u.IsUserMajorDepartment ?? departmentUsers[(u.DepartmentId, userId)].IsUserMajorDepartment;
        }
        else
        {
          if (departments.FirstOrDefault(d => d.Id == u.DepartmentId) != null)
          {
            continue;
          }

          var du = new DepartmentUserEntity
          {
            DepartmentId = u.DepartmentId,
            Position = u.Position,
            Level = u.Level ?? 0,
            IsUserMajorDepartment = u.IsUserMajorDepartment ?? false,
            UserId = userId
          };
          _context.Add(du);
        }

        if (departmentUsers[(u.DepartmentId, userId)].IsUserMajorDepartment)
        {
          departmentUsers.Where(x => x.Key.DepartmentId != u.DepartmentId).ForEach(x => x.Value.IsUserMajorDepartment = false);
        }

        remainMap[(u.DepartmentId, userId)] = true;
      }

      foreach (var du in departmentUsers)
      {
        if (remainMap[du.Key] == false)
        {
          _context.Remove(du.Value);
        }
      }

      await _context.SaveChangesAsync();
    }

    public async Task RemoveUsers(long departmentId, IEnumerable<long> userIds)
    {
      var users = _context.DepartmentUsers.Where(e => e.DepartmentId == departmentId && userIds.Contains(e.UserId));
      _context.DepartmentUsers.RemoveRange(users);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateSeqs(long? parentId, IEnumerable<long> departmentIds)
    {
      var parent = await _context.Departments.FirstAsync(e => e.ParentId == parentId);
      var index = 0;
      var map = departmentIds.ToDictionary(id => id, _ => index++);
      parent.Children.ForEach(c => c.Seq = map[c.Id]);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateStructsAsync(Dictionary<long, IEnumerable<long>> departmentIds)
    {
      var entities = await _context.Departments.ToDictionaryAsync(x => x.Id, x => x);
      foreach (var kv in departmentIds)
      {
        var i = 1;
        foreach (var id in kv.Value)
        {
          entities[id].ParentId = kv.Key;
          entities[id].Seq = i;
          i++;
        }
      }

      _context.UpdateRange(entities.Values);
      await _context.SaveChangesAsync();
    }

    private async Task<Department> EntityToModelAsync(DepartmentEntity entity, Func<DepartmentEntity, Department, Task<Department>>? func = null)
    {
      var model = new Department
      {
        Id = entity.Id,
        Title = entity.Title,
        Children = entity.Children.Any() ? new List<Department>() : null
      };

      if (func is { })
      {
        await func.Invoke(entity, model);
      }

      if (entity.Children.Any())
      {
        await entity.Children.ForEachAsync(async x =>
        {
          var child = await EntityToModelAsync(x, func);
          model.Children!.Add(child);
        });
      }

      return model;
    }
  }
}