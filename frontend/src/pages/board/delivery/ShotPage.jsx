import {
  Button,
  Col,
  DatePicker,
  Divider,
  Empty,
  Form,
  Input,
  message,
  Row,
  Select,
} from "antd";
import { useParams } from "react-router-dom";
import { PageWrapper } from "~/components";
import { ShotSidebar } from "./components/ShotSidebar";
import { TableImport } from "./components/TableImport";
import {
  GetBoard,
  GetShot,
  ListBoardShots,
  RemoveShot,
  StartTurn,
  UpdateShot,
  UpdateShotSettings,
} from "./services";
import { useEffect, useState } from "react";
import { TabLevel } from "./components/TabLevel";
import dayjs from "dayjs";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const ShotPage = () => {
  const { id } = useParams();
  const [shots, setShots] = useState([]);
  const [board, setBoard] = useState({});
  const [activeShotId, setActiveShotId] = useState(-1);
  const [form] = Form.useForm();
  const [turns, setTurns] = useState([]);
  const [currentTurnIndex, setCurrentTurnIndex] = useState(-1);

  useEffect(() => {
    if (id) {
      ListBoardShots(id, (data) => {
        setShots(data);
      });
      GetBoard(id, (data) => {
        setBoard(data);
      });
    }
  }, [id]);

  useEffect(() => {
    setActiveShotId((f) => {
      const i = shots.findIndex((x) => x.id === f);
      if (i > -1) {
        return shots[i].id;
      }
      return shots.length > 0 ? shots[0].id : -1;
    });
  }, [shots]);

  useEffect(() => {
    const shot = shots.find((x) => x.id === activeShotId);
    setCurrentTurnIndex(shot?.currentTurnIndex || -1);
  }, [activeShotId]);

  useEffect(() => {
    if (activeShotId > 0) {
      GetShot(id, activeShotId, (data) => {
        data = { ...data, ...data.settings };
        data.planBeginTime = data.planBeginTime
          ? dayjs(data.planBeginTime)
          : null;
        data.realBeginTime = data.realBeginTime
          ? dayjs(data.realBeginTime)
          : null;
        data.realEndTime = data.realEndTime ? dayjs(data.realEndTime) : null;
        data.powerOnPreparedTime = data.powerOnPreparedTime
          ? dayjs(data.powerOnPreparedTime)
          : null;

        setTurns(data.turns);
        data.turns = data.turns.join("\n");
        form.setFieldsValue(data);
      });
    }
  }, [activeShotId]);
  const handleValuesChange = (values) => {
    let isSetting = false;
    let error = false;
    Object.keys(values).forEach((k) => {
      switch (k) {
        case "planBeginTime":
        case "realBeginTime":
        case "realEndTime":
          values[k] = values[k].format("YYYY-MM-DDTHH:mm:ss");
          break;
        case "powerOnPreparedTime":
          values[k] = values[k].format("YYYY-MM-DDTHH:mm:ss");
          isSetting = true;
          break;
        case "turns":
          values[k] = values[k].trim().split("\n");
          const s = new Set(values[k]);
          if (s.size !== values[k].length) {
            message.error("轮次中有重复内容");
            error = true;
          } else {
            setTurns(values[k]);
          }
          break;
        case "tasksInfo":
          isSetting = true;
          break;
        case "selfProducts":
        case "foreignProducts":
        case "equipments":
        case "engineEquipments":
        case "instruments":
        case "cables":
        case "directorName":
        case "workerName":
          isSetting = true;
          break;
      }
    });
    if (!error) {
      if (isSetting) {
        UpdateShotSettings(id, activeShotId, values);
      } else {
        UpdateShot(id, activeShotId, values);
      }
    }
  };
  return (
    <PageWrapper
      title={`编辑${
        activeShotId >= 0 && shots.length
          ? shots.find((x) => x.id == activeShotId)?.name
          : ""
      }发次`}
      sidebar={
        <ShotSidebar
          board={board}
          activeShotId={activeShotId}
          shots={shots || []}
          onChange={(id) =>
            ListBoardShots(board.id, (data) => {
              setShots(data);
              setActiveShotId(id);
            })
          }
        />
      }
      extras={
        activeShotId > -1 && (
          <>
            <Select
              placeholder="当前轮次"
              width={140}
              value={currentTurnIndex}
              onChange={(v) => setCurrentTurnIndex(v)}
            >
              {turns?.map((x, i) => (
                <Select.Option values={i} key={i}>
                  {x}
                </Select.Option>
              ))}
            </Select>
            <Button
              type="primary"
              onClick={() => {
                StartTurn(board.id, activeShotId, currentTurnIndex);
              }}
              icon={<FontAwesomeIcon icon="play" fixedWidth />}
            >
              启动实时
            </Button>
            <Divider type="vertical" />
            <Button
              type="danger"
              onClick={() => {
                RemoveShot(board.id, activeShotId, () => {
                  ListBoardShots(id, (data) => {
                    setShots(data);
                  });
                });
              }}
              icon={<FontAwesomeIcon icon="trash" fixedWidth />}
            >
              删除发次
            </Button>
          </>
        )
      }
    >
      {activeShotId > -1 ? (
        <Form layout="vertical" form={form} onValuesChange={handleValuesChange}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item label="型号代号" name="name">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="发次代号" name="code">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="批次代号" name="batchCode">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item label="计划开始时间" name="planBeginTime">
                <DatePicker showTime style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="实际开始时间" name="realBeginTime">
                <DatePicker showTime style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="实际结束时间" name="realEndTime">
                <DatePicker showTime style={{ width: "100%" }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item label="上电准备完成时间" name="powerOnPreparedTime">
                <DatePicker showTime style={{ width: "100%" }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="试验指挥" name="directorName">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="箭上岗位" name="workerName">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item label="轮次信息" name="turns">
                <Input.TextArea
                  style={{ height: 240 }}
                  placeholder="每行一条轮次名称，第一条和最后一条作为任务开始和结束。当前轮次为这两个轮次时，看板中不会显示任务和步骤信息"
                />
              </Form.Item>
            </Col>
            <Col span={18}>
              <Form.Item label="任务信息" name="tasksInfo">
                <TabLevel />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item label="本发产品配套" name="selfProducts">
                <TableImport
                  templateName="本发产品配套导入模板"
                  columns={["产品代号", "产品名称", "单机编号"]}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item label="对接产品配套" name="foreignProducts">
                <TableImport
                  templateName="对接产品配套导入模板"
                  columns={[
                    "产品代号",
                    "产品名称",
                    "单机编号"
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item label="测试设备" name="equipments">
                <TableImport
                  templateName="测试设备导入模板"
                  columns={["资产编号", "设备名称", "有效期"]}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item label="测试工装" name="engineEquipments">
                <TableImport
                  templateName="测试工装导入模板"
                  columns={["工装名称", "工装编号", "检查日期"]}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item label="测试电缆" name="cables">
                <TableImport
                  templateName="测试电缆导入模板"
                  columns={["电缆名称", "电缆编号", "检查日期"]}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item label="测试仪表" name="instruments">
                <TableImport
                  templateName="测试仪表导入模板"
                  columns={["资产编号", "仪表名称", "有效期"]}
                />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      ) : (
        <Empty description="请从左边选择发次"></Empty>
      )}
    </PageWrapper>
  );
};
