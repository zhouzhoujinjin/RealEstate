import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Table, Button, Select } from "antd";
import { useEffect, useState } from "react";
import { PageWrapper } from "~/components";
import { DeleteShareScript, GetBriefTasks, GetShareScripts } from "~/services/task";

export const ShareScriptsPage = () => {
  const [tasks, setTasks] = useState([]);
  const [files, setFiles] = useState([]);
  const [currentTask, setCurrentTask] = useState("global");

  useEffect(() => {
    GetBriefTasks((data) => {
      setTasks(data);
    });
  }, []);

  useEffect(() => {
    GetShareScripts(currentTask, (data) => {
      setFiles(data);
    });
  }, [currentTask]);

  return (
    <PageWrapper
      title="测试用例管理"
      extras={
        <Select
          style={{ width: 160 }}
          onChange={(value) => setCurrentTask(value)}
          value={currentTask}
        >
          <Select.Option value="global">公用</Select.Option>
          {tasks.map((x) => (
            <Select.Option key={x.id} value={x.id}>
              {x.modelName}
            </Select.Option>
          ))}
        </Select>
      }
    >
      <Table bordered dataSource={files} size="small" pagination={false}>
        <Table.Column dataIndex="fileName" title="文件名" render={(v) => {
          const parts = v.split('.')
          const ext = parts[parts.length - 1]
          parts.splice(parts.length - 1, 1)
          return <><span>{parts.join('.')}</span><span style={{color: '#ccc'}}>.{ext}</span></>
        }}></Table.Column>
        <Table.Column
          width={164}
          title="操作"
          render={(r) => (
            <>
              <Button
                size="small"
                icon={<FontAwesomeIcon icon="download" fixedWidth />}
                onClick={() => window.location.href = r.url}
              >
                下载
              </Button>
              <Button
                type="danger"
                size="small"
                icon={<FontAwesomeIcon icon="trash" fixedWidth />}
                onClick={() => {
                  DeleteShareScript(r.id, data => {
                    GetShareScripts(currentTask, (data) => {
                      setFiles(data);
                    });
                  });
                }}
              >
                删除
              </Button>
            </>
          )}
        ></Table.Column>
      </Table>
    </PageWrapper>
  );
};
