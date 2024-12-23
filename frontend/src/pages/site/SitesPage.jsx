import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Button, Table } from "antd";
import { useEffect, useState } from "react";
import { PageWrapper } from "~/components";
import { GetSites } from "~/services/task";
import { SiteModal } from "./components/SiteModal";
import { SiteLogModal } from "./components/SiteLogModal";
import { Flex } from "~/components/Flex";

const getStatusTitle = (v) => {
  switch (v) {
    case "Initializing":
      return "初始化中";
    case "Offline":
      return "已离线";
    case "Online":
      return "在线";
    default:
      return "未知";
  }
};

const getCurrentStatusTitle = (v) => {
  switch (v) {
    case 'Unspecified':
      return '未知'
    case "PowerOn":
      return "已加电";
    case "PowerOff":
      return "已断电";
    default:
      return "未知";
  }
};

export const SitesPage = () => {
  const [list, setList] = useState([]);
  const [currentSite, setCurrentSite] = useState();
  const [showLogModal, setShowLogModal] = useState(0);
  useEffect(() => {
    if (!currentSite) {
      GetSites((data) => setList(data));
    }
  }, [currentSite]);

  useEffect(() => {
    if (currentSite) {
    }
  }, [currentSite]);

  return (
    <PageWrapper
      title="工位管理"
      majorAction={
        <Button onClick={() => setCurrentSite({ id: -1 })}>新建工位</Button>
      }
    >
      <Table dataSource={list} rowKey="id">
        <Table.Column title="工位号" dataIndex="siteCode"></Table.Column>
        <Table.Column title="工位名称" dataIndex="name"></Table.Column>
        <Table.Column title="ATP 服务 IP" dataIndex="atpIp"></Table.Column>
        <Table.Column
          title="ATP 项目名称"
          dataIndex="atpProjectName"
        ></Table.Column>
        <Table.Column
          title="监视测点名称"
          dataIndex="atpTppUris"
          render={(v) =>
            v ? (
              <Flex gap={16} justify="flex-start">
                {Object.values(v).map((x, i) => (
                  <span key={i}>{x}</span>
                ))}
              </Flex>
            ) : (
              "未设置"
            )
          }
        ></Table.Column>
        <Table.Column
          title="在线状态"
          render={getStatusTitle}
          dataIndex={["addonInfos", "status"]}
        ></Table.Column>
        <Table.Column
          title="上电状态"
          render={getCurrentStatusTitle}
          dataIndex={["addonInfos", "currentStatus"]}
        ></Table.Column>
        <Table.Column
          title="操作"
          render={(_, row) => (
            <>
              <Button
                onClick={() => setCurrentSite(row)}
                icon={<FontAwesomeIcon icon="pencil" fixedWidth />}
              ></Button>
              <Button
                onClick={() => setShowLogModal(row.id)}
                icon={<FontAwesomeIcon icon="scroll" fixedWidth />}
              ></Button>
            </>
          )}
        ></Table.Column>
      </Table>

      {currentSite && (
        <SiteModal
          site={currentSite}
          onClose={() => setCurrentSite(undefined)}
        />
      )}
      <SiteLogModal siteId={showLogModal} onClose={() => setShowLogModal(0)} />
    </PageWrapper>
  );
};
