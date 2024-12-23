import request from "~/utils/request";
const API_BASE = import.meta.env.VITE_API_BASE;


export const GetYearlyStat = (action) => {
  action &&
    action({
      plan: 89,
      actual: 60,
      shots: [
        {
          name: "ABCD001",
          planDate: "2024-05-01",
          finishedDate: "2024-05-01",
        },
        {
          name: "ABCD002",
          planDate: "2024-06-01",
          finishedDate: "2024-06-04",
        },
        {
          name: "ABCD003",
          planDate: "2024-06-10",
          finishedDate: "2024-06-09",
        },
        {
          name: "ABCD004",
          planDate: "2024-07-01",
          finishedDate: "2024-07-01",
        },
        {
          name: "ABCD005",
          planDate: "2024-08-01",
          finishedDate: "2024-08-01",
        },
        {
          name: "ABCD006",
          planDate: "2024-09-01",
          finishedDate: "2024-09-10",
        },
        {
          name: "ABCD007",
          planDate: "2024-09-11",
          finishedDate: "2024-09-11",
        },
      ],
    });
};

export const GetBoard = async (id, action) => {
  const data = await request.get(`${API_BASE}/delivery-board/${id}`);
  action && action(data);
}

export const GetShot = async (boardId, shotId, action) => {
  const data = await request.get(`${API_BASE}/delivery-board/${boardId}/shots/${shotId}`);
  action && action(data);
  
}
export const GetStat = async (boardId, action) => {
  const data = await request.get(`${API_BASE}/delivery-board/${boardId}/stat`);
  action && action(data);
  
}


