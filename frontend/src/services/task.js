import request from "~/utils/request";
import { sortBy } from "lodash";

const API_BASE = import.meta.env.VITE_API_BASE;

export const GetBriefTasks = async (action) => {
  const data = await request.get(`${API_BASE}/tasks/brief`);
  action && action(data);
};

export const GetBriefTasksByUser = async (action) => {
  const data = await request.get(`${API_BASE}/tasks/briefByUser`);
  action && action(data);
};

export const GetTask = async (taskId, action) => {
  const data = await request.get(`${API_BASE}/tasks/${taskId}`);
  action && action(data);
};

export const GetTaskCheckRecord = async (taskId, action) => {
  const data = await request.get(`${API_BASE}/tasks/${taskId}/checkRecord`);
  action && action(data);
};

export const GetBriefStages = async (taskId, action) => {
  const data = await request.get(`${API_BASE}/tasks/${taskId}/stages/brief`);
  action && action(data);
};
export const GetBriefProjects = async (taskId, stageId, action) => {
  const data = await request.get(
    `${API_BASE}/tasks/${taskId}/stages/${stageId}/projects/brief`
  );
  action && action(data);
};

export const GetProject = async (projectId, action) => {
  const data = await request.get(`${API_BASE}/projects/${projectId}`);
  action?.(data);
};

export const CreateProject = async (
  { stageId, atpId, atpName, projectId, projectName },
  action
) => {
  const url = projectId
    ? `${API_BASE}/projects/${projectId}/update`
    : `${API_BASE}/projects`;

  const data = await request.post(url, {
    stageId,
    atpId,
    projectName,
    atpName: atpName || projectName,
  });
  action && action(data);
};

export const UpdateProject = async (projectId, project, action) => {
  const data = await request.post(
    `${API_BASE}/projects/${projectId}/update`,
    project
  );
  action && action(data);
};

export const RemoveProject = async (projectId, action) => {
  const data = await request.post(`${API_BASE}/projects/${projectId}/delete`);
  action && action(data);
};

export const GetProjectEquipments = async (projectId, action) => {
  const data = await request.get(
    `${API_BASE}/projects/${projectId}/equipments`
  );
  action && action(data);
};
export const GetProjectLocations = async (projectId, action) => {
  const data = await request.get(`${API_BASE}/projects/${projectId}/locations`);
  action && action(data);
};

export const GetProjectUsers = async (projectId, action) => {
  const data = await request.get(`${API_BASE}/projects/${projectId}/users`);
  action && action(data);
};

export const GetAtpProject = async (atpId, taskInfo, action) => {
  const data = await request.get(`${API_BASE}/projects/${atpId}`, {
    params: taskInfo,
  });
  action && action(data);
};

export const InfoCallback = async (id, action) => {
  const data = await request.post(`${API_BASE}/projects/${id}/infoback`);

  action && data && action(data);
};

export const GetProjectFiles = async (projectId, action) => {
  const data = await request.get(`${API_BASE}/projects/${projectId}/files`);

  action && action(data);
};

export const RemoveProjectFile = async (projectId, fileId, action) => {
  const data = await request.post(
    `${API_BASE}/projects/${projectId}/files/${fileId}/delete`
  );
  action && action(data);
};

export const GetShareScripts = async (taskId, action) => {
  const id = taskId == "global" ? "0" : taskId;
  const data = await request.get(`${API_BASE}/shareScripts/${id}`);

  action && action(data);
};

export const DeleteShareScript = async (fileId, action) => {
  const data = await request.post(`${API_BASE}/shareScripts/${fileId}/delete`);
  action && action(data);
};

export const GetSites = async (action) => {
  const data = await request.get(`${API_BASE}/sites`);
  action && action(data);
};

export const GetSiteLogs = async (siteId, action) => {
  const data = await request.get(`${API_BASE}/sites/${siteId}/logs`);
  action && action(data);
};

export const CreateSite = async (site, action) => {
  const data = await request.post(`${API_BASE}/sites`, site);
  action && action(data);
};

export const UpdateSite = async (site, action) => {
  const data = await request.post(`${API_BASE}/sites/${site.id}`, site);
  action && action(data);
};

export const GetAtpProjects = async (ip, action) => {
  const data = await request.get(`${API_BASE}/atp-servers/${ip}`);
  action && action(data);
};

export const GetCategories = async (ip, projectId, action) => {
  const data = await request.get(
    `${API_BASE}/atp-servers/${ip}/projects/${projectId}/categories`
  );
  action && action(data);
  return data;
};

export const GetTestPoints = async (serverName, projectId, action) => {
  const data = await request.get(
    `${API_BASE}/atp-servers/${serverName}/projects/${projectId}/testPoints`
  );
  action && action(data);
  return data;
};

export const GetAllTestPoints = async (ip, projectId, action) => {
  const categories = await GetCategories(ip, projectId);
  const testpoints = await GetTestPoints(ip, projectId);

  const map = new Map();
  const list = [];
  categories.forEach((x) => {
    map.set(x.id, {
      key: x.id,
      label: x.name,
      value: x.id,
      children: [],
    });
  });

  categories.forEach((x) => {
    if (x.parentId && x.parentId !== "00000000-0000-0000-0000-000000000000") {
      const parent = map.get(x.parentId);
      parent.children.push(map.get(x.id));
    } else {
      list.push(map.get(x.id));
    }
  });

  sortBy(testpoints, (x) => x.uriString).forEach((x) => {
    if (x.categoryId) {
      const parent = map.get(x.categoryId);
      const testPoint = {
        key: x.id,
        value: x.id,
        label: x.name,
        uri: x.uriString,
        children: x.properties.map((y) => ({
          key: y.id,
          value: y.id,
          label: y.name,
          uri: y.uriString,
        })),
      };

      if (!parent.children) {
        parent.children = [];
      }
      parent.children.push(testPoint);
    }
  });
  action && action(list);
  return list;
};

export const SyncProjectToTdm = async (projectId, action) => {
  const data = await request.post(`${API_BASE}/sync-tdm/projects/${projectId}`);
  action && action(data);
};

export const SyncStageToTdm = async (stageId, action) => {
  const data = await request.post(`${API_BASE}/sync-tdm/stages/${stageId}`);
  action && action(data);
};

export const GetTaskFiles = async (taskId, action) => {
  const data = await request.get(`${API_BASE}/tasks/${taskId}/files`);

  action && action(data);
};

export const GetProjectBySiteId = async (siteId, action) => {
  const data = await request.get(`${API_BASE}/sites/${siteId}/project`);
  action && action(data);
};
