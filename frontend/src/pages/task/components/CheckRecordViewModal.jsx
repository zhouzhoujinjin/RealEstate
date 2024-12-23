import { Modal, Typography } from "antd";
import { useEffect, useState } from "react";
import { GetTaskCheckRecord } from "~/services/task";

export const CheckRecordViewModal = ({ taskId, open, onClose }) => {
  const [checkRecord, setCheckRecord] = useState({});
  useEffect(() => {
    GetTaskCheckRecord(taskId, (data) => {
      setCheckRecord(data || {});
    });
  }, []);

  return (
    <Modal
      open={true}
      title="查看检查记录"
      onCancel={() => onClose()}
      footer={null}
    >
      <Typography>
        {checkRecord.record?.split("\n").map((x) => (
          <Typography.Paragraph>{x}</Typography.Paragraph>
        ))}
      </Typography>
    </Modal>
  );
};
