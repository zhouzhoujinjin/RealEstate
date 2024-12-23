import { PageWrapper } from "~/components";
import { ModelSidebar } from "./components/ModelSidebar";
import { useEffect, useState } from "react";
import {
  GetBriefTasks,
  GetBriefTasksByUser,
  GetTask,
  SyncProjectToTdm,
  SyncStageToTdm,
} from "~/services/task";
import { Button, Descriptions, Table, Tag, Tooltip } from "antd";
import dayjs from "dayjs";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { CheckRecordViewModal } from "./components/CheckRecordViewModal";
import { CheckRecordEditModal } from "./components/CheckRecordEditModal";
import { ProjectModal } from "./components/ProjectModal";
import { ProjectFileModal } from "./components/ProjectFileModal";
import { Flex } from "~/components/Flex";
import { TaskFileModal } from "./components/TaskFileModal";
import { useAuth } from "~/hooks";

let placeholderId = -1;

const createProjectPlaceholder = (project) => ({
  id: placeholderId--,
  projectName: "",
  planBeginDate: project.planBeginDate,
  planEndDate: project.planEndDate,
  projectBeginDate: "",
  projectEndDate: "",
  powerOn: "",
  atpId: "",
  atpName: "",
  version: "",
  taskId: project.taskId,
  stageId: project.stageId,
  stageName: project.stageName,
  modelName: project.modelName,
});

const getTagColor = (status) => {
  switch (status) {
    case 0:
      return "default";
    case 1:
      return "processing";
    case 2:
      return "success";
    case 3:
      return "error";
    default:
      return "default";
  }
};

const getTagTitle = (status) => {
  switch (status) {
    case 0:
      return "未开始";
    case 1:
      return "进行中";
    case 2:
      return "已完成";
    case 3:
      return "已取消";
    default:
      return "未开始";
  }
};

const getTaskStatusIcon = (status) => {
  switch (status) {
    case 1:
    case "Reviewing":
      return "hourglass";
    case 2:
    case "Processing":
      return "bolt";
    case 3:
    case "Finished":
      return "octagon-check";
    default:
      return "ballot-check";
  }
};

export const TasksPage = () => {
  const [tasks, setTasks] = useState([]);
  const [activeTaskId, setActiveTaskId] = useState(0);
  const [activeTask, setActiveTask] = useState();
  const [projects, setProjects] = useState([]);
  const { user } = useAuth();
  const [showCheckRecordEditModal, setShowCheckRecordEditModal] =
    useState(false);
  const [showCheckRecordViewModal, setShowCheckRecordViewModal] =
    useState(false);
  const [showProjectModal, setShowProjectModal] = useState(false);
  const [showProjectFileModal, setShowProjectFileModal] = useState(false);
  const [currentProjectId, setCurrentProjectId] = useState();
  const [showTaskFileModal, setShowTaskFileModal] = useState(false);

  const handleSyncProjectToTdm = (projectId) => {
    SyncProjectToTdm(projectId);
  };

  const handleSyncStageToTdm = (stageId) => {
    SyncStageToTdm(stageId);
  };
  useEffect(() => {
    if (user.userName === 'admin') {
      GetBriefTasks((data) => {
        setTasks(data);
        setActiveTaskId(data[0].id);
      });
    } else {
      GetBriefTasksByUser((data) => {
        setTasks(data);
        setActiveTaskId(data[0].id);
      });
    }
  }, []);

  useEffect(() => {
    if (!activeTaskId) return;
    GetTask(activeTaskId, (data) => {
      setActiveTask(data);

      const p = [];

      if (data.stages.length === 0) {
      } else {
        data.stages.forEach((stage) => {
          if (stage.projects.length === 0) {
            const pp = createProjectPlaceholder({
              taskId: data.id,
              stageId: stage.id,
              planBeginDate: stage.planBeginDate,
              planEndDate: stage.planEndDate,
              stageName: stage.name,
            });
            pp.stageRowSpan = 1;
            p.push(pp);
          } else {
            stage.projects.forEach((project, i) => {
              project.planBeginDate = stage.planBeginDate;
              project.planEndDate = stage.planEndDate;
              project.stageName = stage.name;
              if (i === 0) {
                project.stageRowSpan = stage.projects.length;
              }
              p.push(project);
            });
          }
        });
      }
      setProjects(p);
    });
  }, [activeTaskId, tasks]);

  return (
    <>
      <PageWrapper
        title={`${activeTask?.modelName || "加载中..."} (${
          activeTask?.uniqueCode || ""
        })`}
        sidebar={
          <ModelSidebar
            tasks={tasks}
            activeTaskId={activeTaskId}
            onChange={setActiveTaskId}
          />
        }
        extras={
          <>
            <Button
              icon={<FontAwesomeIcon icon="files" fixedWidth />}
              onClick={() => setShowTaskFileModal(true)}
            >
              查看项目文件
            </Button>
            <Button.Group>
              <Button
                icon={
                  <FontAwesomeIcon
                    icon={getTaskStatusIcon(activeTask?.taskStatus)}
                    fixedWidth
                  />
                }
                onClick={() => setShowCheckRecordViewModal(true)}
              >
                试验检查记录
              </Button>
              {activeTask?.taskStatus === "Preparing" && (
                <Button
                  icon={<FontAwesomeIcon icon="edit" fixedWidth />}
                  onClick={() => setShowCheckRecordEditModal(true)}
                />
              )}
            </Button.Group>
          </>
        }
      >
        <Descriptions layout="vertical">
          <Descriptions.Item label="型号人员">
            <Flex direction="column" align="flex-start">
              {activeTask?.users?.map((x) => (
                <span key={x.id}>
                  {x.user} ({x.position})
                </span>
              ))}
            </Flex>
          </Descriptions.Item>
          <Descriptions.Item label="配套设备">
            <Flex direction="column" align="flex-start">
              {activeTask?.equipments?.map((x) => (
                <span key={x.equipmentCode}>
                  {x.equipmentName} ({x.equipmentCode})
                </span>
              ))}
            </Flex>
          </Descriptions.Item>
          <Descriptions.Item label="配套工位">
            <Flex direction="column" align="flex-start">
              {activeTask?.locations?.map((x) => (
                <span key={x.locationCode}>
                  {x.locationInfo} ({x.locationCode})
                </span>
              ))}
            </Flex>
          </Descriptions.Item>
        </Descriptions>
      </PageWrapper>
      <PageWrapper style={{ marginLeft: 220 }}>
        <Table
          bordered
          dataSource={projects}
          size="small"
          pagination={false}
          rowKey={"id"}
        >
          <Table.Column
            title="阶段"
            dataIndex="stageName"
            key="stageName"
            onCell={(r) => ({ rowSpan: r.stageRowSpan || 0 })}
            render={(v, r) =>
              r.id < 0 ? (
                v
              ) : (
                <Flex justify="flex-start" align="center">
                  <span>{v}</span>
                  <Tooltip title="回传 TDM">
                    <Button
                      size="small"
                      onClick={() => {
                        handleSyncStageToTdm(r.stageId);
                      }}
                      style={{ marginLeft: "8px" }}
                      icon={
                        <FontAwesomeIcon icon="cloud-arrow-up" fixedWidth />
                      }
                    ></Button>
                  </Tooltip>
                </Flex>
              )
            }
          />
          <Table.Column
            title="计划开始时间"
            dataIndex="planBeginDate"
            key="planBeginDate"
            width={105}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
            onCell={(r) => ({ rowSpan: r.stageRowSpan || 0 })}
          />
          <Table.Column
            title="计划结束时间"
            dataIndex="planEndDate"
            key="planEndDate"
            width={105}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
            onCell={(r) => ({ rowSpan: r.stageRowSpan || 0 })}
          />
          <Table.Column
            title="试验计划"
            dataIndex="projectName"
            key="projectName"
            render={(v, r) => {
              return (
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                  }}
                >
                  <Button
                    type="link"
                    onClick={() => {
                      setCurrentProjectId(r.id);
                      setShowProjectModal(true);
                    }}
                  >
                    {v}
                  </Button>
                  {r.id > 0 && (
                    <div>
                      <Tooltip title="回传 TDM">
                        <Button
                          size="small"
                          icon={<FontAwesomeIcon icon="cloud-arrow-up" />}
                          onClick={() => {
                            handleSyncProjectToTdm(r.id);
                          }}
                        />
                      </Tooltip>
                      <Tooltip title="查看文件">
                        <Button
                          size="small"
                          icon={<FontAwesomeIcon icon="files" />}
                          onClick={() => {
                            setCurrentProjectId(r.id);
                            setShowProjectFileModal(true);
                          }}
                        />
                      </Tooltip>
                    </div>
                  )}
                </div>
              );
            }}
          />
          <Table.Column
            title="项目计划开始时间"
            dataIndex="projectPlanBeginDate"
            key="projectPlanBeginDate"
            width={130}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
          />
          <Table.Column
            title="项目计划结束时间"
            dataIndex="projectPlanEndDate"
            key="projectPlanEndDate"
            width={130}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
          />
          <Table.Column
            title="实际开始时间"
            dataIndex="projectBeginDate"
            key="projectBeginDate"
            width={105}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
          />
          <Table.Column
            title="实际结束时间"
            dataIndex="projectEndDate"
            key="projectEndDate"
            width={105}
            render={(v) => {
              return v && dayjs(v).format("YYYY-MM-DD");
            }}
          />
          <Table.Column
            title="版本信息"
            dataIndex="version"
            key="version"
            width={73}
          />
          <Table.Column
            title="状态"
            dataIndex="status"
            key="status"
            width={64}
            render={(v, r) =>
              r.id < 0 ? null : (
                <Tag color={getTagColor(v)}>{getTagTitle(v)}</Tag>
              )
            }
          />
        </Table>
        {showCheckRecordViewModal && (
          <CheckRecordViewModal
            taskId={activeTaskId}
            onClose={() => setShowCheckRecordViewModal(false)}
          />
        )}
        {showCheckRecordEditModal && (
          <CheckRecordEditModal
            taskId={activeTaskId}
            onClose={() => setShowCheckRecordEditModal(false)}
          />
        )}

        {showProjectModal && (
          <ProjectModal
            taskUsers={activeTask?.users || []}
            open={showProjectModal}
            taskId={activeTaskId}
            projectId={currentProjectId}
            onClose={(refresh = false) => {
              setShowProjectModal(false);
              if (refresh) {
                GetTask(activeTaskId, (data) => {
                  setActiveTask(data);

                  const p = [];

                  if (data.stages.length === 0) {
                  } else {
                    data.stages.forEach((stage) => {
                      if (stage.projects.length === 0) {
                        const pp = createProjectPlaceholder({
                          taskId: data.id,
                          stageId: stage.id,
                          planBeginDate: stage.planBeginDate,
                          planEndDate: stage.planEndDate,
                          stageName: stage.name,
                        });
                        pp.stageRowSpan = 1;
                        p.push(pp);
                      } else {
                        stage.projects.forEach((project, i) => {
                          project.planBeginDate = stage.planBeginDate;
                          project.planEndDate = stage.planEndDate;
                          project.stageName = stage.name;
                          if (i === 0) {
                            project.stageRowSpan = stage.projects.length;
                          }
                          p.push(project);
                        });
                      }
                    });
                  }
                  setProjects(p);
                });
              }
            }}
          />
        )}
        {showProjectFileModal && (
          <ProjectFileModal
            projectId={currentProjectId}
            onClose={() => setShowProjectFileModal(false)}
          />
        )}
        {showTaskFileModal && (
          <TaskFileModal
            taskId={activeTaskId}
            onClose={() => setShowTaskFileModal(false)}
          />
        )}
      </PageWrapper>
    </>
  );
};
