import request from "~/utils/request";
const API_BASE = import.meta.env.VITE_API_BASE;

export const GetBriefBoards = async (action) => {
  const data = await request.get(`${API_BASE}/delivery-board/boards`);
  action && action(data);
};

export const GetBoard = async (id, action) => {
  const data = await request.get(`${API_BASE}/delivery-board/${id}`);
  action && action(data);
};

export const UpdateBoard = async (id, board, action) => {
  const data = await request.post(`${API_BASE}/delivery-board/${id}`, board);
  action && action(data);
};
export const UpdateShot = async (boardId, shotId, shot, action) => {
  const data = await request.post(
    `${API_BASE}/delivery-board/${boardId}/shots/${shotId}`,
    shot
  );
  action && action(data);
};
export const UpdateShotSettings = async (
  boardId,
  shotId,
  shotSettings,
  action
) => {
  const data = await request.post(
    `${API_BASE}/delivery-board/${boardId}/shots/${shotId}/settings`,
    shotSettings
  );
  action && action(data);
};

export const StartTurn = async (boardId, shotId, turnIndex, action) => {
  const data = await request.post(
    `${API_BASE}/delivery-board/${boardId}/shots/${shotId}/start-turn`,
    { turnIndex }
  );
  action && action(data);
};


export const RemoveShot = async (boardId, shotId, action) => {
  const data = await request.post(
    `${API_BASE}/delivery-board/${boardId}/shots/${shotId}/delete`
  );
  action && action(data);
}

export const GetAtpServers = async (action) => {
  const data = await request.get(`${API_BASE}/atp-servers`);
  action && action(data);
};

export const CreateShot = async ({ boardId, name, copyId }, action) => {
  const data = await request.post(
    `${API_BASE}/delivery-board/${boardId}/shots`,
    {
      name,
      copyId,
      boardId,
    }
  );
  action && action(data);
};

export const ListBoardShots = async (boardId, action) => {
  const data = await request.get(`${API_BASE}/delivery-board/${boardId}/shots`);
  action && action(data);
};

export const GetShot = async (boardId, shotId, action) => {
  const data = await request.get(
    `${API_BASE}/delivery-board/${boardId}/shots/${shotId}`
  );
  action && action(data);
};
