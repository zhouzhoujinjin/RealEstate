import { Button, Col, DatePicker, Form, Input, Modal, Row, Select } from "antd";
import React, { useEffect, useState } from "react";
import dayjs from "dayjs";

import {
  GetBriefStages,
  GetProject,
  GetBriefTasks,
  UpdateProject,
  RemoveProject,
} from "~/services/task";

export const ProjectModal = ({
  projectId,
  taskUsers,
  onClose,
  taskId,
  ...rest
}) => {
  const [form] = Form.useForm();
  const [currentStage, setCurrentStage] = useState();
  const [stages, setStages] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [project, setProject] = useState();

  useEffect(() => {
    if (projectId > 0) {
      GetProject(projectId, (data) => setProject(data));
    }
  }, [projectId]);

  useEffect(() => {
    if (project) {
      setCurrentStage(project.stageId);
      form.setFieldsValue({
        ...project,
        projectPlanBeginDate: dayjs(project.projectPlanBeginDate || undefined),
        projectPlanEndDate: dayjs(project.projectPlanEndDate || undefined),
        projectBeginDate: dayjs(project.projectBeginDate),
        projectEndDate: dayjs(project.projectEndDate),
        powerOn: dayjs(project.powerOn).isValid()
          ? dayjs(project.powerOn)
          : null,
      });
    }
  }, [project]);

  useEffect(() => {
    GetBriefTasks((data) => setTasks(data));
  }, []);

  useEffect(() => {
    if (taskId && tasks.find((x) => x.id === taskId)) {
      GetBriefStages(taskId, (data) => setStages(data));
    }
  }, [taskId, tasks]);

  const onRemove = (project) => {
    RemoveProject(project.id, () => {
      onClose(true);
    });
  };

  return (
    <Modal
      title="项目信息"
      width="75%"
      {...rest}
      onCancel={onClose}
      footer={
        <div style={{ display: "flex", justifyContent: "space-between" }}>
          <Button
            onClick={() => {
              onRemove && project && onRemove(project);
            }}
          >
            删除项目
          </Button>
          <Button
            onClick={() => {
              const values = form.getFieldsValue();
              values.projectPlanBeginDate = values.projectPlanBeginDate.format(
                "YYYY-MM-DDTHH:mm:ss"
              );
              values.projectPlanEndDate = values.projectPlanEndDate.format(
                "YYYY-MM-DDTHH:mm:ss"
              );
              values.projectBeginDate = values?.projectBeginDate.format(
                "YYYY-MM-DDTHH:mm:ss"
              );
              values.projectEndDate = values?.projectEndDate.format(
                "YYYY-MM-DDTHH:mm:ss"
              );
              values.powerOn = values.powerOn?.format("YYYY-MM-DDTHH:mm:ss");
              UpdateProject(
                projectId,
                {
                  ...project,
                  ...values,
                },
                (data) => {
                  onClose();
                }
              );
            }}
          >
            保存
          </Button>
        </div>
      }
    >
      <Form form={form} layout="vertical">
        <Row gutter={24}>
          <Col span={12}>
            <Form.Item label="型号名称">
              {tasks.find((x) => x.id === taskId)?.modelName}
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item label="阶段名称">
              {stages.find((x) => x.id === project?.stageId)?.name}
            </Form.Item>
          </Col>
          <Col span={4}>
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
            <Form.Item label="阶段计划时间">
              {currentStage
                ? `${dayjs(currentStage.planBeginDate).format(
                    "YYYY-MM-DD HH:mm"
                  )} ~ ${dayjs(currentStage.planEndDate).format(
                    "YYYY-MM-DD HH:mm"
                  )}`
                : "请先选择阶段"}
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Atp 项目" labelCol={{ span: 4 }}>
              {project?.atpName || ""}
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
                {taskUsers.map((x) => (
                  <Select.Option value={x.id} key={x.id}>
                    {x.user}
                  </Select.Option>
                ))}
              </Select>
            </Form.Item>
          </Col>
        </Row>
      </Form>
    </Modal>
  );
};
