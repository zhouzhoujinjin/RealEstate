import { Button, Table } from "antd";
import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";

import { PageWrapper } from "~/components";
function useQuery() {
  const { search } = useLocation();

  return React.useMemo(() => new URLSearchParams(search), [search]);
}
export const CheckRecordsPage = () => {
  const query = useQuery();
  const id = query.get('id');
  const [showModal, setShowModal] = useState(false);
  useEffect(() => {
    if (id) {
      setShowModal(true);
    }
  }, [id]);
  console.log(showModal);
  return (
    <PageWrapper>
      <Table rowKey='modelName'>
        <Table.Column title="型号" dataIndex="modelName" width={200}></Table.Column>
        <Table.Column title="摘要" dataIndex="modelName"></Table.Column>
        <Table.Column
          title="审核状态"
          dataIndex="approvalStatus"
          width={120}
        ></Table.Column>
        <Table.Column
          title="操作"
          width={80}
          dataIndex="modelName"
          render={(v, r) => <Button>编辑</Button>}
        ></Table.Column>
      </Table>
    </PageWrapper>
  );
};
