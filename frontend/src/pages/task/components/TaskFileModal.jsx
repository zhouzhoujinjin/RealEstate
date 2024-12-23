import { useEffect, useState } from "react";
import { Button, Modal, Table, Upload, message } from "antd";

import { EditableCell } from "../../atp/components/EditableCell";
import { GetTaskFiles } from "~/services/task";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const TaskFileModal = ({ taskId, onClose }) => {
  const [list, setList] = useState([]);
  useEffect(() => {
    if (taskId > 0) {
      GetTaskFiles(taskId, (r) => {
        setList(r);
      });
    }
  }, [taskId]);

  const remove = (id) => {
    console.log(id);
  };

  return (
    <Modal
      title="项目文件"
      onCancel={onClose}
      footer={
        <Upload
          showUploadList={false}
          name="file"
          action={`/api/tasks/${taskId}/files`}
          onChange={({ file }) => {
            if (file.status === "done") {
              const newData = [
                ...list,
                {
                  id: file.response.data.id,
                  fileType: 2,
                  fileName: file.response.data.fileName,
                  filePath: file.response.data.filePath,
                },
              ];
              setList(newData);
              message.success({
                content: "上传成功",
                duration: 2,
              });
            }
          }}
        >
          <Button>上传文件</Button>
        </Upload>
      }
      open
      bodyStyle={{ padding: 0 }}
    >
      <Table
        style={{ width: "100%" }}
        dataSource={list}
        pagination={false}
        rowKey="id"
        components={{
          body: {
            cell: EditableCell,
          },
        }}
      >
        <Table.Column title="序号" render={(_, __, i) => i + 1} width={60} />
        <Table.Column title="文件名" dataIndex="fileName" />
        <Table.Column
          title="操作"
          width={80}
          render={(_, __, i) => {
            return (
              <Button
                size="small"
                icon={<FontAwesomeIcon icon="trash" fixedWidth />}
                onClick={() => remove(i)}
              ></Button>
            );
          }}
        />
      </Table>
    </Modal>
  );
};
