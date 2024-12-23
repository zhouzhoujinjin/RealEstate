import { EChart } from "@kbox-labs/react-echarts";
import styles from "./FinishedStat.module.css";
import ProgressBar from "@ramonak/react-progress-bar";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useEffect, useState } from "react";
import { Accordion } from "@mantine/core";
import { GetProjectBySiteId, GetTask } from "~/services/task";
import { GetSiteCurrentStat } from "../services/satellite";

const pieData = [
  {
    label: "系统供配电",
    value: 66.67,
  },
  {
    label: "单机接入系统",
    value: 77.73,
  },
  {
    label: "系统开环",
    value: 40,
  },
  {
    label: "系统闭环",
    value: 60,
  },
  {
    label: "故障模拟",
    value: 33.33,
  },
  {
    label: "其他测试",
    value: 33.33,
  },
];
export const FinishedStat = ({ site }) => {
  const [model, setModel] = useState({
    stages: [],
  });
  const [currentProject, setCurrentProject] = useState({});
  const [currentStage, setCurrentStage] = useState({});
  const [pieData, setPieData] = useState([]);
  const [monthCurrent, setMonthCurrent] = useState([]);

  const [showEquipModal, setShowEquipModal] = useState(false);
  const [showProductModal, setShowProductModal] = useState(false);
  const [showTeamModal, setShowTeamModal] = useState(false);

  useEffect(() => {
    if (site) {
      GetProjectBySiteId(site.id, (project) => {
        setCurrentProject(project || {});
        if (project?.taskId) {
          GetTask(project.taskId, (task) => {
            setModel(task);
            const newPieData = [];
            task.stages.forEach((s) => {
              if (s.projects.findIndex((p) => p.id == project.id) > -1) {
                setCurrentStage(s);
              }
              const pie = {
                label: s.name,
                value: s.projects.length
                  ? (
                      (s.projects.filter((x) => x.status == 2).length /
                        s.projects.length) *
                      100
                    ).toFixed(2)
                  : 0,
              };
              newPieData.push(pie);
            });
            setPieData(newPieData);
          });
        } else {
          setModel({
            modelName: "",
            stages: [],
          });
          setPieData([]);
          setCurrentStage({});
        }
      });
      GetSiteCurrentStat(site.id, (data) => {
        setMonthCurrent(data.map((x) => x / 3600));
      });
    }
  }, [site]);
  return (
    <div className="card">
      <h4>测试项目完成情况</h4>
      <div className={styles.cells}>
        <div className={styles.cell}>
          <div className={styles.cellContent}>
            工位号：
            <span style={{ fontSize: "1.1em", fontWeight: "bold" }}>
              {site?.name}
            </span>
          </div>
        </div>
        <div className={styles.cell}>
          <div className={styles.cellContent}>
            正在进行的试验项目：
            <span style={{ fontSize: "1.1em", fontWeight: "bold" }}>
              {currentStage.name} - {currentProject?.projectName}
            </span>
          </div>
        </div>
        <div className={styles.cell}>
          <div className={styles.cellContent}>
            当日参试人员：
            <span style={{ fontSize: "1.1em", fontWeight: "bold" }}>
              {currentProject?.users?.map((x) => x.userName).join(", ") || "--"}
            </span>
          </div>
        </div>
      </div>
      <div className={styles.cells}>
        <EChart
          theme="dark"
          backgroundColor="transparent"
          style={{ height: 280, width: 420 }}
          grid={{ top: 10, left: 10, bottom: 0, right: 0, containLabel: true }}
          legend={{
            show: true,
            data: ["加电时长"],
            left: "right",
            top: "top",
          }}
          xAxis={{
            type: "category",
            data: [
              "1月",
              "2月",
              "3月",
              "4月",
              "5月",
              "6月",
              "7月",
              "8月",
              "9月",
              "10月",
              "11月",
              "12月",
            ],
          }}
          yAxis={{
            type: "value",
            axisLabel: {
              formatter: "{value} 小时",
            },
          }}
          series={[
            {
              name: "加电时长",
              data: monthCurrent,
              type: "bar",
            },
          ]}
        />
        <table className={styles.table} border={0}>
          <tbody>
            <tr>
              <td>{currentProject.projectName || "无"}</td>
              <td>{model.modelName?.split(":")[0] || "无"}</td>
            </tr>
            <tr>
              <td>研制阶段</td>
              <td>{currentStage.name}</td>
            </tr>
            <tr>
              <td>技术负责人</td>
              <td></td>
            </tr>
            <tr>
              <td>测试负责人</td>
              <td></td>
            </tr>
          </tbody>
        </table>
        <div className={styles.buttons}>
          <button
            className={styles.button}
            onClick={() => setShowProductModal(true)}
          >
            <span>产品配套表</span>
            <FontAwesomeIcon icon="external-link" fixedWidth />
          </button>
          <button
            className={styles.button}
            onClick={() => setShowEquipModal(true)}
          >
            <span>设备配套表</span>
            <FontAwesomeIcon icon="external-link" fixedWidth />
          </button>
          <button
            className={styles.button}
            onClick={() => setShowTeamModal(true)}
          >
            <span>团队信息</span>
            <FontAwesomeIcon icon="external-link" fixedWidth />
          </button>
        </div>
      </div>
      <div className={styles.cells}>
        <div className={styles.bar}>
          <div className={styles.barLabel}>型号测试时间完成情况</div>
          <div className={styles.barContent}>
            <ProgressBar
              completed={150}
              className={styles.progressWrapper}
              bgColor="#91cc75"
              baseBgColor="rgba(255,255,255,0.2)"
            />
          </div>
        </div>
        <div className={styles.bar}>
          <div className={styles.barLabel}>型号测试项目完成情况</div>
          <div className={styles.barContent}>
            <ProgressBar
              completed={40}
              className={styles.progressWrapper}
              bgColor="#4992ff"
              baseBgColor="rgba(255,255,255,0.2)"
            />
          </div>
        </div>
      </div>
      <div
        className={styles.cells}
        style={{ marginLeft: 10, alignItems: "flex-start" }}
      >
        <Accordion style={{ width: 240 }}>
          {model?.stages.map((item) => (
            <Accordion.Item key={item.name} value={item.name}>
              <Accordion.Control>{item.name}</Accordion.Control>
              <Accordion.Panel>
                {item.projects.length > 0
                  ? item.projects.map((p) => (
                      <p key={p.id} style={{ marginLeft: 20 }}>
                        {p.projectName}
                      </p>
                    ))
                  : "无项目"}
              </Accordion.Panel>
            </Accordion.Item>
          ))}
        </Accordion>
        <div className={styles.pies}>
          {pieData.map((x) => (
            <div className={styles.pie} key={x.label}>
              <EChart
                style={{ width: 240, height: 240 }}
                key={x.label}
                series={[
                  {
                    type: "gauge",
                    startAngle: 90,
                    endAngle: -270,
                    pointer: {
                      show: false,
                    },
                    data: [x.value],
                    axisTick: {
                      show: false,
                    },
                    axisLabel: {
                      show: false,
                    },
                    splitLine: {
                      show: false,
                    },
                    progress: {
                      show: true,
                      overlap: false,
                      roundCap: true,
                      clip: false,
                    },
                    detail: {
                      width: 50,
                      height: 14,
                      fontSize: 14,
                      color: "#fff",
                      formatter: "{value}%\n完成率",
                    },
                  },
                ]}
              />
              <p>{x.label}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
