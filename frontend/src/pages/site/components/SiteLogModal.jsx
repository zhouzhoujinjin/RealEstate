import { Modal, Table } from "antd";
import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { GetSiteLogs } from "~/services/task";
const getTypeTitle = (v) => {
  switch (v) {
    case "Initializing":
      return "初始化中";
    case "Offline":
      return "已离线";
    case "Online":
      return "在线";
    case "PowerOn":
      return "已加电";
    case "PowerOff":
      return "已断电";
  }
};
export const SiteLogModal = ({ siteId, onClose }) => {
  const [list, setList] = useState([]);
  useEffect(() => {
    if (siteId > 0) {
      GetSiteLogs(siteId, setList);
    }
  }, [siteId]);

  return (
    <Modal
    width='80%'
      title="上电日志"
      open={siteId > 0}
      onCancel={() => onClose()}
      footer={null}
      bodyStyle={{ maxHeight: "80%", overflow: "auto" }}
    >
      <Table dataSource={list} rowKey="id" pagination={false}>
        <Table.Column
          title="事件时间"
          dataIndex="eventTime"
          render={(v) => dayjs(v).format("YYYY-MM-DD HH:mm:ss")}
        ></Table.Column>
        <Table.Column
          title="记录时间"
          dataIndex="logTime"
          render={(v) => dayjs(v).format("YYYY-MM-DD HH:mm:ss")}
        ></Table.Column>
        <Table.Column
          title="事件类型"
          dataIndex="eventType"
          render={(v) => getTypeTitle(v)}
        ></Table.Column>
        <Table.Column title="数据" dataIndex="content"></Table.Column>
      </Table>
    </Modal>
  );
};
