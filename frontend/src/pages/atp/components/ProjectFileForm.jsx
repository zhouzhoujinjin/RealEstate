import React, { useEffect, useState } from "react";
import { message, Button, Card, Table, Upload } from "antd";

import { EditableCell } from "./EditableCell";
import { GetProjectFiles } from "~/services/task";
import { DeleteOutlined } from "@ant-design/icons";

export const ProjectFileForm = ({ projectId }) => {
  const [list, setList] = useState([]);
  useEffect(() => {
    if (projectId > 0) {
      GetProjectFiles(projectId, (r) => {
        setList(r);
      });
    }
  }, [projectId]);

  const remove = (id) => {
    console.log(id);
  };

  return (
    <Card
      style={{ width: "100%" }}
      styles={{
        body: {
          padding: 0,
        },
      }}
      bordered={false}
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
        footer={() => {
          return (
            <Upload
              showUploadList={false}
              name="file"
              action={`/api/projects/${projectId}/files`}
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
                  message?.success({
                    content: "上传成功",
                    duration: 2,
                  });
                }
              }}
            >
              <Button>上传文件</Button>
            </Upload>
          );
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
                icon={<DeleteOutlined />}
                onClick={() => remove(i)}
              ></Button>
            );
          }}
        />
      </Table>
    </Card>
  );
};
