import { Button, Col, Form, Input, message, Row } from "antd";
import { PageWrapper } from "~/components";
import { TableImport } from "../delivery/components/TableImport";
import { useEffect, useState } from "react";
import {
  GetSatelliteBoardSetting,
  SaveSatelliteBoardSetting,
} from "./services";
import {
  formItemLayout,
  formItemLayoutWithoutLabel,
} from "~/utils/formLayouts";

const columns = ["型号","实际", "计划"];
export const SettingPage = () => {
  const [form] = Form.useForm();
  const [modelList, setModelList] = useState([]);
  useEffect(() => {
    GetSatelliteBoardSetting((data) => {
      setModelList(data.modelList);
      form.setFieldValue("modelList", data.modelList.join("\n"));
      form.setFieldValue("yearStat", data.yearStat);
    });
  }, []);

  const handleValuesChange = (values) => {
    if (values.modelList) {
      setModelList(values.modelList.split("\n").filter((x) => x));
    }
  };

  const handleSubmit = (values) => {
    values.modelList = modelList;
    SaveSatelliteBoardSetting(values);
  };

  return (
    <PageWrapper title="卫星看板设置">
      <Form
        layout="horizontal"
        form={form}
        onValuesChange={handleValuesChange}
        onFinish={handleSubmit}
      >
        <Form.Item
          label="型号列表"
          name="modelList"
          help="一行一个型号"
          {...formItemLayout}
        >
          <Input.TextArea style={{ width: "100%", height: "5em" }} />
        </Form.Item>
        <Form.Item
          label="年度统计信息"
          name="yearStat"
          help="请先设置型号列表"
          {...formItemLayout}
        >
          <TableImport
            templateName="测试设备导入模板"
            columns={columns}
            initialRows={modelList.map((x) => [x])}
          />
        </Form.Item>
        <Form.Item {...formItemLayoutWithoutLabel}>
          <Button type="primary" onClick={() => form.submit()}>
            保存
          </Button>
        </Form.Item>
      </Form>
    </PageWrapper>
  );
};
