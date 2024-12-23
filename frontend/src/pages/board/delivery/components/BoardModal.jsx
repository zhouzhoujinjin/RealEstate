import { Form, Input, Modal, Select } from "antd";
import { useEffect, useState } from "react";
import { GetSites } from "~/services/task";
import { UpdateBoard } from "../services";
import { ImageUpload } from "~/components/ImageUpload";

export const BoardModal = ({ board, onClose }) => {
  const [form] = Form.useForm();
  const [sites, setSites] = useState([]);

  useEffect(() => {
    GetSites((data) => setSites(data));
  }, []);

  useEffect(() => {
    if (board) {
      if(!board.addonInfos) {
        board.addonInfos = {}
      }
      if(!board.addonInfos.background) board.addonInfos.background = ""
      if(!board.addonInfos.rocket) board.addonInfos.rocket = ""
      
      form.setFieldsValue({ ...board });
    }
  }, [board, sites]);

  return (
    <Modal
      open={board?.id > 0}
      onCancel={() => onClose()}
      onOk={() => {
        const data = form.getFieldsValue();
        if (board?.id) {
          UpdateBoard(board.id, data, () => {
            onClose();
          });
        }
      }}
    >
      <Form layout="vertical" form={form}>
        <Form.Item label="看板名称" name="name">
          <Input />
        </Form.Item>
        <Form.Item label="ATP 服务器" name="siteId">
          <Select>
            {sites.map((x) => (
              <Select.Option key={x.siteCode} value={x.id}>
                {x.name} - {x.siteCode} ({x.atpIp})
              </Select.Option>
            ))}
          </Select>
        </Form.Item>
        <Form.Item label="看板背景" name={["addonInfos", "background"]}>
          <ImageUpload action="/api/delivery-board/upload" />
        </Form.Item>
        <Form.Item label="看板火箭" name={["addonInfos", "rocket"]}>
          <ImageUpload action="/api/delivery-board/upload" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
