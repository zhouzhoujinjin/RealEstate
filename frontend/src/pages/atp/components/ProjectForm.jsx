import { Card, Col, DatePicker, Form, Input, Row, Select } from "antd";
import React, { useEffect, useState } from "react";
import { GetBriefStages, GetBriefTasks, GetTask, UpdateProject } from "~/services/task";
import dayjs from "dayjs";

export const ProjectForm = ({ project }) => {
  const [form] = Form.useForm();
  const [taskName, setTaskName] = useState("");
  const [stageName, setStageName] = useState("");
  const [users, setUsers] = useState([]);
  const [task, setTask] = useState({})
  useEffect(() => {
    if (project) {
      GetBriefTasks((data) => {
        setTaskName(data.find((x) => x.id === project.taskId)?.modelName);
      });
      GetBriefStages(project.taskId, (data) => {
        setStageName(data.find((x) => x.id === project.stageId)?.name);
      });
      project.projectBeginDate = project.projectBeginDate
        ? dayjs(project.projectBeginDate)
        : undefined;

      project.projectEndDate = project.projectEndDate
        ? dayjs(project.projectEndDate)
        : undefined;

      project.projectPlanBeginDate = project.projectPlanBeginDate
        ? dayjs(project.projectPlanBeginDate)
        : undefined;
      project.projectPlanEndDate = project.projectPlanEndDate
        ? dayjs(project.projectPlanEndDate)
        : undefined;
      project.powerOn = project.powerOn ? dayjs(project.powerOn) : undefined;
      form.setFieldsValue(project);

      GetTask(project.taskId, data => setTask(data))
    }
  }, [project]);

  return (
    <Card style={{ width: "100%" }} bordered={false}>
      <Form
        form={form}
        layout="horizontal"
        onValuesChange={(v) => {
          if (project) {
            Object.keys(v).forEach((k) => {
              if (
                [
                  "powerOn",
                  "projectPlanBeginDate",
                  "projectPlanEndDate",
                  "projectBeginDate",
                  "projectEndDate",
                ].includes(k)
              ) {
                UpdateProject(
                  project?.id,
                  // @ts-expect-error 在的
                  { [k]: v[k].format("YYYY-MM-DDTHH:mm:ss") },
                  () => {}
                );
              } else {
                UpdateProject(project.id, v, () => {});
              }
            });
          }
        }}
      >
        <Row gutter={24}>
          <Col span={12}>
            <Form.Item label="型号名称" name="taskId" labelCol={{ span: 6 }}>
              {taskName}
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="阶段名称" labelCol={{ span: 6 }}>
              {stageName}
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={18}>
            <Form.Item
              name="projectName"
              label="项目名称"
              labelCol={{ span: 4 }}
            >
              <Input />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="version" label="版本信息">
              <Input />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={18}>
            <Form.Item name="atpId" label="Atp 项目" labelCol={{ span: 4 }}>
              {`${project?.atpName}`}
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="status" label="任务状态">
              <Select>
                <Select.Option value={0}>待开始</Select.Option>
                <Select.Option value={1}>正在进行</Select.Option>
                <Select.Option value={2}>已完成</Select.Option>
                <Select.Option value={3}>已取消</Select.Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={12}>
            <Form.Item
              name="projectPlanBeginDate"
              label="计划开始时间"
              labelCol={{ span: 6 }}
            >
              <DatePicker
                showTime
                format="YYYY-MM-DD HH:mm:ss"
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="projectPlanEndDate"
              label="计划结束时间"
              labelCol={{ span: 6 }}
            >
              <DatePicker
                showTime
                format="YYYY-MM-DD HH:mm:ss"
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={12}>
            <Form.Item
              name="projectBeginDate"
              label="实际开始时间"
              labelCol={{ span: 6 }}
            >
              <DatePicker
                showTime
                format="YYYY-MM-DD HH:mm:ss"
                readOnly
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="projectEndDate"
              label="实际结束时间"
              labelCol={{ span: 6 }}
            >
              <DatePicker
                showTime
                format="YYYY-MM-DD HH:mm:ss"
                readOnly
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={24}>
          <Col span={12}>
            <Form.Item
              name="powerOnTimeSpan"
              label="上电时长"
              labelCol={{ span: 6 }}
            >
              <Input readOnly />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="userIds" label="操作人员" labelCol={{ span: 6 }}>
              <Select mode="multiple" style={{ width: "100%" }}>
                {task?.users?.map((x) => (
                  <Select.Option value={x.id} key={x.id}>
                    {x.user}
                  </Select.Option>
                ))}
              </Select>
            </Form.Item>
          </Col>
        </Row>
      </Form>
    </Card>
  );
};
