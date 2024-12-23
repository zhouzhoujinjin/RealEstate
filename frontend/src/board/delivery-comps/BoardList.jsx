import { Badge, Button } from "@mantine/core";
import { useEffect, useState } from "react";
import { GetBriefBoards } from "~/pages/board/delivery/services";
import styles from "./BoardList.module.css";
export const BoardList = ({ boardsCurrent }) => {
  const [boards, setBoards] = useState([]);
  useEffect(() => {
    GetBriefBoards((data) => {
      setBoards(data);
    });
  }, []);
  return (
    <>
      <h2>运载看板列表</h2>
      <div className={styles.gallery}>
        {boards.map((x) => (
          <div className={styles.item} key={x.id}>
            <img
              src={x.addonInfos.background}
              onClick={() => (window.location.href = `?id=${x.id}`)}
            />
            <h4
              style={{
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                gap: 12,
              }}
            >
              {x.name}
              <Badge
                size="xl"
                color={boardsCurrent[x.siteId] === "PowerOn" ? "green" : "red"}
              >
                {boardsCurrent[x.siteId] === "PowerOn" ? "已上电" : "未上电"}
              </Badge>
            </h4>
          </div>
        ))}
      </div>
    </>
  );
};
