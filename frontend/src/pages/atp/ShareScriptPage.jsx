import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Table, Button, Select } from "antd";
import React, { useEffect, useState } from "react";
import { GetBriefTasks, GetShareScripts } from "~/services/task";

export const ShareScriptPage = () => {
  const [tasks, setTasks] = useState([]);
  const [files, setFiles] = useState([]);
  const [currentTask, setCurrentTask] = useState("global");
  const [temp, setTemp] = useState("");
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
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 8,
        width: "100%",
        padding: 8,
      }}
    >
      {temp}
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
      <Table bordered dataSource={files} rowKey='url' size="small" pagination={false}>
        <Table.Column
          dataIndex="fileName"
          title="文件名"
          render={(v) => {
            const parts = v.split(".");
            const ext = parts[parts.length - 1];
            parts.splice(parts.length - 1, 1);
            return (
              <>
                <span>{parts.join(".")}</span>
                <span style={{ color: "#ccc" }}>.{ext}</span>
              </>
            );
          }}
        ></Table.Column>
        <Table.Column
          width={164}
          title="操作"
          render={(_, r) => (
            <>
              <Button
                size="small"
                icon={<FontAwesomeIcon icon="download" fixedWidth />}
                onClick={() => {
                  setTemp(r.url);
                  console.log(window.cef)
                  //window.cef.helloWorld();
                  window.cef.importFieldScript(r.url);
                }}
              >
                下载
              </Button>
            </>
          )}
        ></Table.Column>
      </Table>
    </div>
  );
};
