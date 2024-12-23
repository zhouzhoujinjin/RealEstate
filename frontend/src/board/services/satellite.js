import dayjs from "dayjs";
import request from "~/utils/request";
const API_BASE = import.meta.env.VITE_API_BASE;

export const GetYearStat = async (action) => {
  const data = await request.get(`/api/satellite/yearStat`);

  action && action(data);
};

export const GetRightData = async (modelId, action) => {
  const data = await request.post(`/system/board/dataRight`, {
    modelId,
  });

  action && action(data);
};

export const GetEnv = async (action) => {
  action &&
    action({
      hum: Math.random() * 10 + 30,
      tem: Math.random() * 18 + 8,
    });
};

export const GetSystemStartTime = async (action) => {
  const data = await request.get(`/system/server/runtime`);
  const startTime = dayjs().subtract(data["runtime"], "second");
  action && action(startTime);
};

export const GetSites = async (action) => {
  const data = await request.get(`${API_BASE}/satellite/sites`);
  action && action(data);
  return data;
};

export const GetSiteCurrentStat = async (siteId, action) => {
  const data = await request.get(
    `${API_BASE}/satellite/sites/${siteId}/currentStat`
  );
  action && action(data);
};
