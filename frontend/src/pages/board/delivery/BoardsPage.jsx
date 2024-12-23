import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Button, Modal, Table } from "antd";
import { useEffect, useState } from "react";
import { PageWrapper } from "~/components";
import { GetBriefBoards } from "./services";
import { useTabLayout } from "~/hooks";
import { useHistory } from "react-router-dom";
import { BoardModal } from "./components/BoardModal";

export const BoardsPage = () => {
  const [list, setList] = useState([]);
  const history = useHistory();
  const [currentBoard, setCurrentBoard] = useState();

  const { addTab } = useTabLayout();
  useEffect(() => {
    GetBriefBoards((data) => setList(data));
  }, []);

  return (
    <PageWrapper title="看板管理">
      <Table dataSource={list} rowKey="id">
        <Table.Column title="看板名称" dataIndex="name" />
        <Table.Column title="工位名称" dataIndex="siteName" />
        <Table.Column
          width={300}
          title="操作"
          render={(_, r) => {
            return (
              <>
                <Button
                  icon={<FontAwesomeIcon icon="pencil" fixedWidth />}
                  onClick={() => {
                    setCurrentBoard(r);
                  }}
                >
                  编辑
                </Button>
                <Button
                  onClick={() => {
                    addTab({
                      key: `/deliveryBoards/${r.id}/shots`,
                      title: `${r.name} 看板发次`,
                      prev: "/deliveryBoards",
                    });
                    history.push(`/deliveryBoards/${r.id}/shots`);
                  }}
                  icon={<FontAwesomeIcon icon="rocket" fixedWidth />}
                >
                  发次
                </Button>
                <Button icon={<FontAwesomeIcon icon="trash" fixedWidth />}>
                  删除
                </Button>
              </>
            );
          }}
        />
      </Table>
      <BoardModal
        board={currentBoard}
        onClose={() => {
          setCurrentBoard(undefined);
          GetBriefBoards((data) => setList(data));
        }}
      ></BoardModal>
    </PageWrapper>
  );
};
