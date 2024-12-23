import { Input, Modal, Typography } from "antd";
import { useEffect, useState } from "react";
import { GetTaskCheckRecord } from "~/services/task";

export const CheckRecordEditModal = ({ taskId, open, onClose }) => {
  const [checkRecord, setCheckRecord] = useState({});
  useEffect(() => {
    GetTaskCheckRecord(taskId, (data) => {
      setCheckRecord(data || {});
    });
  }, []);

  return (
    <Modal
      open={true}
      title="编辑检查记录"
      onCancel={() => onClose()}
      footer={null}
      bodyStyle={{height: 400}}
    >
      <Input.TextArea
        style={{ width: "100%", height: "100%" }}
        value={checkRecord.record}
        onChange={(e) =>
          setCheckRecord((f) => ({ ...f, record: e.target.value }))
        }
      ></Input.TextArea>
    </Modal>
  );
};
