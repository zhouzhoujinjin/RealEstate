import React, { useEffect, useMemo, useState } from "react";

import { message, Tabs } from "antd";
import { ProjectForm } from "./components/ProjectForm";
import { ProjectFileForm } from "./components/ProjectFileForm";
import { useLocation, useParams } from "react-router-dom";
import { GetAtpProject } from "../..//services/task";
import { NewProjectForm } from "./components/NewProjectForm";

function useQuery() {
  const { search } = useLocation();

  return React.useMemo(() => new URLSearchParams(search), [search]);
}

export const TaskPage = () => {
  const [project, setProject] = useState();
  const items = useMemo(() => {
    return [
      {
        label: "信息",
        key: "info",
        children: <ProjectForm project={project} />,
      },
      {
        label: "试验文件",
        key: "files",
        children: <ProjectFileForm projectId={project?.id || 0} />,
      },
    ];
  }, [project]);
  const [activeKey, setActiveKey] = useState("info");
  const { hash } = useLocation();
  const { atpId } = useParams();

  const query = useQuery();
  const token = query.get('token')
  const stageId = query.get('stageid')
  const taskName = query.get('taskName')
  useEffect(() => {
    setActiveKey(hash.replace(/^#/, "") || "info");
  }, [hash, atpId]);

  useEffect(() => {
    if (!atpId) {
      setExists(false);
      message.error('请求地址有误')
      return;
    }
    GetAtpProject(atpId, {stageId, taskName, token}, (data) => {
      setProject(data);
    });
  }, [atpId]);
  return (
    <div
      style={{
        width: "100%",
        margin: "4px 8px",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <Tabs
        activeKey={activeKey}
        items={items}
        centered
        style={{ width: "100%" }}
        onTabClick={(ak) => setActiveKey(ak)}
      />
    </div>
  );
};
