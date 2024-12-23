import ReactDOM from "react-dom/client";

import "@mantine/core/styles.css";
import { useEffect, useMemo, useState } from "react";
import { Badge, Flex, MantineProvider, Tabs } from "@mantine/core";
import { Card } from "./delivery-comps/Card";
import { CurrentTime } from "./delivery-comps/CurrentTime";
import { HeadTitle } from "./delivery-comps/HeadTitle";
import { TemperatureHumidity } from "./delivery-comps/TemperatureHumidity";
import styles from "./deliveryBoardPage.module.less";
import { ShotSteps } from "./delivery-comps/ShotSteps";
import { ShotTurns } from "./delivery-comps/ShotTurns";
import { ShotProjects } from "./delivery-comps/ShotProjects";
import { RocketImage } from "./delivery-comps/RocketImage";
import { BoardTable } from "./delivery-comps/BoardTable";

import { config } from "@fortawesome/fontawesome-svg-core";
import { GetBoard, GetShot, GetStat } from "./services/delivery";
import dayjs from "dayjs";
import * as signalR from "@microsoft/signalr";
import { BoardList } from "./delivery-comps/BoardList";

config.styleDefault = "light";

const getId = () => {
  const urlParams = new URLSearchParams(window.location.search);
  const myParam = urlParams.get("id");
  return myParam;
};

const SignalrUri = import.meta.env.VITE_SIGNALR_URI;

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${SignalrUri}/realtime`, {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets,
  })
  .configureLogging(signalR.LogLevel.Information)
  .build();

async function start() {
  try {
    await connection.start();
    console.log("SignalR Connected.");
  } catch (err) {
    console.log(err);
  }
}
start();

const max = (arr) => {
  return arr.length ? arr.sort()[arr.length - 1] : "--";
};

export const DeliveryBoardPage = () => {
  const id = useMemo(getId, [window.location.search]);
  const [board, setBoard] = useState();
  const [shot, setShot] = useState();
  const [yearStat, setYearStat] = useState({});
  const [tasks, setTasks] = useState([]);
  const [steps, setSteps] = useState([]);
  const [boardsCurrent, setBoardsCurrent] = useState({});
  useEffect(() => {
    connection.on("currentChange", (code, status, id) => {
      setBoardsCurrent((b) => ({ ...b, [id]: status }));
    });
    if (id) {
      GetBoard(id, (data) => {
        setBoard(data);
      });
      return () => {
        if (connection.state == signalR.HubConnectionState.Connected) {
          connection.invoke("LeaveGroup", "board", id);
          connection.off("currentChange");
        }
      };
    }

    return () => {
      connection.off("currentChange");
    };
  }, [id]);
  
  useEffect(() => {
    if (board) {
      connection.invoke("JoinGroup", "board", board.id);
      connection.on("taskChange", (data) => {
        setShot(data);
        setTasks(data.settings.tasksInfo.taskNames);
      });
      connection.on("stepChange", (data) => {
        setShot(data);
        setTasks(data.settings.tasksInfo.taskNames);
      });
      if (board.currentShotId > 0) {
        GetShot(board.id, board.currentShotId, (data) => {
          setShot(data);
          setTasks(data.settings.tasksInfo.taskNames);
        });
        GetStat(board.id, (data) => {
          const all = data.length;
          const finished = data.filter((x) => x.realEndTime).length;
          const records = data.map((x) => ({
            name: x.name,
            planDate: dayjs(x.planBeginTime).format("YYYY-MM-DD"),
            finishedDate: x.realEndTime
              ? dayjs(x.realEndTime).format("YYYY-MM-DD")
              : "--",
          }));
          setYearStat({
            all,
            finished,
            records,
          });
        });
      }
      return () => {
        connection.invoke("LeaveGroup", board.id);
      };
    }
  }, [board]);

  useEffect(() => {
    if (shot?.currentTaskIndex > -1) {
      setSteps(
        shot.settings.tasksInfo.taskSteps[
          shot.settings.tasksInfo.taskNames[shot.currentTaskIndex].code
        ]
      );
    }
  }, [shot?.currentTaskIndex]);

  return (
    <MantineProvider defaultColorScheme="dark">
      {id && (
        <div className={styles.board}>
          <div className={styles.head}>
            <div className={styles.headLeft}>
              <CurrentTime />
            </div>
            <div className={styles.headCenter}>
              <HeadTitle title={shot?.name} />
            </div>
            <div className={styles.headRight}>
              <TemperatureHumidity />
            </div>
          </div>
          <div className={styles.main}>
            <div className={styles.mainLeft}>
              <Card title="年度任务完成情况" style={{ height: 595 }}>
                <Flex
                  justify="space-between"
                  fz={20}
                  mt={10}
                  ml={20}
                  mr={20}
                  mb={20}
                >
                  <div>年度： {yearStat.all}</div>
                  <div>已完成： {yearStat.finished}</div>
                </Flex>
                <BoardTable
                  rowKey="shot"
                  columns={[
                    { name: "name", title: "发次" },
                    { name: "planDate", title: "计划时间" },
                    { name: "finishedDate", title: "完成时间" },
                  ]}
                  elements={yearStat.records || []}
                />
              </Card>
              <Card title="本发任务完成情况">
                <BoardTable
                  rowKey="title"
                  columns={[
                    { name: "title", title: "" },
                    { name: "value", title: "" },
                  ]}
                  elements={[
                    {
                      title: "任务开始时间",
                      value:
                        shot && shot.realBeginTime
                          ? dayjs(shot.realBeginTime).format("YYYY-MM-DD HH:mm")
                          : "--",
                    },
                    {
                      title: "加电前准备完成",
                      value:
                        shot && shot.settings.powerOnPreparedTime
                          ? dayjs(shot.settings.powerOnPreparedTime).format(
                              "YYYY-MM-DD HH:mm"
                            )
                          : "--",
                    },
                    {
                      title: "测试项目完成",
                      value: `${
                        shot?.settings?.runtimeTasks?.filter((x) => x.finished)
                          .length || 0
                      }条`,
                    },
                    {
                      title: "测试项目加电时间",
                      value: board?.realEndTime
                        ? dayjs(board.realEndTime)
                            .diff(dayjs(board.realBeginTime))
                            .format("dDhHm[m]")
                        : "--",
                    },
                    {
                      title: "任务结束时间",
                      value: board?.realEndTime || "--",
                    },
                  ]}
                />
              </Card>
            </div>
            <div className={styles.mainCenter}>
              <Flex
                justify="center"
                align="center"
                style={{ fontSize: 30, textAlign: "center" }}
              >
                {shot?.batchCode} / {shot?.code}{" "}
                <Badge
                  color={
                    board && boardsCurrent[board.siteId] === "PowerOn"
                      ? "green"
                      : " red"
                  }
                  size="xl"
                  style={{ marginLeft: 20 }}
                >
                  {board && boardsCurrent[board.siteId] === "PowerOn"
                    ? "正在通电"
                    : "已断电"}
                </Badge>
              </Flex>
              <ShotTurns
                current={shot?.currentTurnIndex}
                items={shot?.turns || []}
              />
              <div className={styles.mainBody}>
                <ShotProjects
                  current={shot?.currentTaskIndex}
                  items={shot?.settings.runtimeTasks || []}
                />
                <RocketImage src={board?.addonInfos?.rocket || "/cz6a.png"} />
                <ShotSteps
                  current={shot?.currentStepIndex}
                  items={
                    shot?.settings.runtimeTasks[shot?.currentTaskIndex]
                      ?.steps || []
                  }
                />
              </div>
            </div>
            <div className={styles.mainRight}>
              <Card title="试验人员管理">
                <Flex justify="space-between" fz={20} mt={10} ml={20} mr={20}>
                  <div>试验指挥： {shot?.settings.directorName}</div>
                  <div>箭上岗位： {shot?.settings.workerName}</div>
                </Flex>
              </Card>
              <Card title="试验产品管理" style={{ height: 360 }}>
                <Tabs defaultValue="selfProducts">
                  <Tabs.List>
                    <Tabs.Tab fz={20} value="selfProducts">
                      本发产品配套
                    </Tabs.Tab>
                    <Tabs.Tab fz={20} value="foreignProducts">
                      对接产品配套
                    </Tabs.Tab>
                  </Tabs.List>
                  <Tabs.Panel value="selfProducts">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "产品代号" },
                        { name: 1, title: "产品名称" },
                        { name: 2, title: "单机编号" },
                        { name: 3, title: "安装位置" },
                      ]}
                      elements={shot?.settings?.selfProducts || []}
                    />
                  </Tabs.Panel>

                  <Tabs.Panel value="foreignProducts">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "单机名称" },
                        { name: 1, title: "产品代号" },
                        { name: 2, title: "产品编号" },
                        { name: 3, title: "评审时间" },
                      ]}
                      elements={shot?.settings?.foreignProducts || []}
                    />
                  </Tabs.Panel>
                </Tabs>
              </Card>
              <Card title="试验设备管理" style={{ height: 360 }}>
                <Tabs defaultValue="equipments">
                  <Tabs.List>
                    <Tabs.Tab fz={20} value="equipments">
                      测试设备
                    </Tabs.Tab>
                    <Tabs.Tab fz={20} value="engineEquipments">
                      工装设备
                    </Tabs.Tab>
                    <Tabs.Tab fz={20} value="cables">
                      测试电缆
                    </Tabs.Tab>
                    <Tabs.Tab fz={20} value="instruments">
                      测试仪表
                    </Tabs.Tab>
                  </Tabs.List>
                  <Tabs.Panel value="equipments">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "资产编号" },
                        { name: 1, title: "设备名称" },
                        { name: 2, title: "型号代号" },
                        { name: 3, title: "有效期" },
                      ]}
                      elements={shot?.settings?.equipments || []}
                    />
                  </Tabs.Panel>

                  <Tabs.Panel value="engineEquipments">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "工装名称" },
                        { name: 1, title: "工装编号" },
                        { name: 2, title: "是否有效" },
                        { name: 3, title: "检查日期" },
                      ]}
                      elements={shot?.settings?.engineEquipments || []}
                    />
                  </Tabs.Panel>
                  <Tabs.Panel value="cables">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "电缆名称" },
                        { name: 1, title: "电缆编号" },
                        { name: 2, title: "是否自检" },
                        { name: 3, title: "检查日期" },
                      ]}
                      elements={shot?.settings?.cables || []}
                    />
                  </Tabs.Panel>
                  <Tabs.Panel value="instruments">
                    <BoardTable
                      fz={20}
                      mt={10}
                      rowKey={0}
                      columns={[
                        { name: 0, title: "资产编号" },
                        { name: 1, title: "仪表名称" },
                        { name: 2, title: "型号代号" },
                        { name: 3, title: "有效期" },
                      ]}
                      elements={shot?.settings?.instruments || []}
                    />
                  </Tabs.Panel>
                </Tabs>
              </Card>
            </div>
          </div>
        </div>
      )}
      {!id && (
        <div className={styles.board}>
          <BoardList boardsCurrent={boardsCurrent} />
        </div>
      )}
    </MantineProvider>
  );
};

ReactDOM.createRoot(document.getElementById("root")).render(
  <DeliveryBoardPage />
);
