import request from "~/utils/request";
import { API } from "~/config";

export const GetSatelliteBoardSetting = async (action) => {
  const data = await request.get(`${API}/satellite/setting`);
  action && action(data);
};

export const SaveSatelliteBoardSetting = async (config, action) => {
  const data = await request.post(`${API}/satellite/setting`, config);
  action && action(data);
};
