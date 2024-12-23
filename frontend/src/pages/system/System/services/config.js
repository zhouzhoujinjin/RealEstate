import request from '~/utils/request'
import { API } from '~/config'

export const SaveConfig = async (config, action) => {
  const data = await request.put(`${API}/admin/config`, config)
  action(data)
}
