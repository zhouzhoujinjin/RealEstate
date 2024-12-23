import { Button, Card, Checkbox, Col, Form, Input, Row, Select } from "antd";
import React, { useEffect, useState } from "react";
import {
  CreateProject,
  GetBriefProjects,
  GetBriefStages,
  GetBriefTasks,
} from "~/services/task";
import { useHistory } from "react-router-dom";

export const NewProjectForm = ({ atpId, atpName, stageId }) => {
  const [form] = Form.useForm();
  const [currentTaskId, setCurrentTaskId] = useState("");
  const [currentStageId, setCurrentStageId] = useState(stageId || "");
  const [currentProjectId, setCurrentProjectId] = useState(0);
  const [create, setCreate] = useState(false);
  const [tasks, setTasks] = useState([]);
  const [stages, setStages] = useState([]);
  const [projects, setProjects] = useState([]);
  const history = useHistory();

  useEffect(() => {
    GetBriefTasks((data) => setTasks(data));
  }, []);
  useEffect(() => {
    if (currentTaskId) {
      GetBriefStages(currentTaskId, (data) => setStages(data));
    } else {
      setStages([]);
    }
    setCurrentProjectId("");
    setCurrentStageId("");
  }, [currentTaskId]);

  useEffect(() => {
    if (currentTaskId && currentStageId) {
      GetBriefProjects(currentTaskId, currentStageId, (data) =>
        setProjects(data)
      );
    }
  }, [currentTaskId, currentStageId]);

  const handleFinish = (values) => {
    CreateProject(
      {
        stageId: currentStageId,
        atpId,
        atpName,
        projectId: create ? undefined : currentProjectId,
        projectName:
          +values.projectName == values.projectName
            ? projects.find((x) => x.id === currentProjectId)?.projectName
            : values.projectName,
      },
      (data) => {
        console.log(data);
        if (data) {
          window.location.reload();
        }
      }
    );
  };
  return (
    <Card style={{ width: "100%" }} bordered={false}>
      <Form
        form={form}
        layout="horizontal"
        size="small"
        onFinish={handleFinish}
      >
        <Row gutter={24}>
          <Col span={20}>
            <Form.Item label="型号名称" name="taskId" labelCol={{ span: 4 }}>
              <Select
                onChange={(v) => {
                  setCurrentTaskId(v);
                }}
              >
                {tasks.map((x) => (
                  <Select.Option key={x.id} value={x.id}>
                    {x.modelName}
                  </Select.Option>
                ))}
              </Select>
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={20}>
            <Form.Item label="阶段名称" name="stageId" labelCol={{ span: 4 }}>
              <Select
                onChange={(v) => {
                  setCurrentStageId(v);
                }}
              >
                {stages.map((x) => (
                  <Select.Option key={x.id} value={x.id}>
                    {x.name}
                  </Select.Option>
                ))}
              </Select>
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={20}>
            <Form.Item
              label="任务名称"
              name="projectName"
              labelCol={{ span: 4 }}
            >
              {create ? (
                <Input />
              ) : (
                <Select onChange={(v) => setCurrentProjectId(v)}>
                  {projects.map((x) => (
                    <Select.Option key={x.id} value={x.id}>
                      {x.projectName}
                    </Select.Option>
                  ))}
                </Select>
              )}
            </Form.Item>
          </Col>
          <Col span={4}>
            <Form.Item>
              <Checkbox onChange={(e) => setCreate(e.target.checked)}>
                或创建新的任务
              </Checkbox>
            </Form.Item>
          </Col>
        </Row>
        <Row>
          <Col span={24} offset={4}>
            <Button htmlType="submit">确定</Button>
          </Col>
        </Row>
      </Form>
    </Card>
  );
};
