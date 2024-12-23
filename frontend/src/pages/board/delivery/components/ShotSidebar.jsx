import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Flex } from "~/components/Flex";
import { Button, Form, Input, List, Popover, Select } from "antd";
import { CreateShot } from "../services";

const NewShotForm = ({ shots, form }) => {
  return (
    <Form form={form}>
      <Form.Item name="name" label="发次名称" labelCol={{ span: 7 }}>
        <Input />
      </Form.Item>
      {/* <Form.Item name="copy-from" label="复制自" labelCol={{ span: 7 }}>
        <Select>
          <Select.Option value="">无</Select.Option>
          {shots.map((x) => (
            <Select.Option value={x.id} key={x.id}>
              {x.name}
            </Select.Option>
          ))}
        </Select>
      </Form.Item> */}
    </Form>
  );
};

export const ShotSidebar = ({ shots = [], board, activeShotId, onChange }) => {
  const [form] = Form.useForm();
  return (
    <List size="large">
      <Flex
        justify="space-between"
        style={{
          padding: 8,
          borderBottom: "1px solid #cfcfcf",
        }}
      >
        <div
          style={{
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
            overflow: "hidden",
          }}
        >
          {board?.name}发次列表
        </div>
        <Popover
          placement="bottomRight"
          arrowPointAtCenter
          title={
            <Flex justify="space-between">
              <div>新建发次</div>
              <Button
                size="small"
                type="primary"
                onClick={() => {
                  const shot = form.getFieldsValue();
                  shot.boardId = board?.id;
                  CreateShot(shot, (data) => onChange(data.id));
                }}
                icon={<FontAwesomeIcon icon="save" fixedWidth />}
              ></Button>
            </Flex>
          }
          content={<NewShotForm shots={shots} form={form} />}
          trigger="click"
        >
          <Button
            size="small"
            icon={<FontAwesomeIcon icon="plus" fixedWidth />}
          />
        </Popover>
      </Flex>
      {shots.map((x) => (
        <List.Item
          key={x.id}
          onClick={() => onChange(x.id)}
          style={
            activeShotId === x.id
              ? { background: "#f0f2f5", fontWeight: "bold", cursor: "pointer" }
              : { cursor: "pointer" }
          }
        >
          <span>{x.name} ({x.code})</span>
        </List.Item>
      ))}
    </List>
  );
};
