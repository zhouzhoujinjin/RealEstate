import React, { useState, useEffect } from "react";
import {  Row,  Col,  Button,  Tree,  Card,  Input,  Form,  message,  Select,  Dropdown,} from "antd";
import { PageWrapper } from "~/components/PageWrapper";
import {  GetDepartments,  GetDepartment,  AddDepartment,  UpdateDepartment,  DeleteDepartment,} from "./services/department";
import "./style.less";
import { DepartmentUserInput } from "./components";
import { GetUsers } from "~/services/utils";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {  formItemLayout,  formItemLayoutWithoutLabel,} from "~/utils/formLayouts";
import { loop } from "~/utils/utils";

let departmentMap = {};

const NodeForm = (props) => {
  const { value, optionUsers } = props;
  const [form] = Form.useForm();
  useEffect(() => {
    form.setFieldsValue({
      title: value.title,
      abbreviation: value.abbreviation,
      users: value.users,
      userIds: value.users ? value.users.map((x) => x.id) : [],
    });
  }, [form, value]);

  const onFinish = async (values) => {
    try {
      await form.validateFields(Object.keys(values));
      props.onChange(value.path, {
        ...value,
        ...values,
      });
    } catch {}
  };

  const addOrRemoveUsers = (v) => {
    const oldUsers = form.getFieldsValue()["users"];
    const newUsers = [];
    v.forEach((x) => {
      const existedUser = oldUsers ? oldUsers.find((y) => y.id === x) : null;
      if (existedUser) {
        newUsers.push(existedUser);
      } else {
        const ou = optionUsers.find((y) => y.value === x);
        if (ou) {
          newUsers.push({
            id: ou.value,
            userName: `${ou.option.userName}`,
            profiles: { fullName: ou.option.fullName },
            position: "员工",
            level: 0,
          });
        }
      }
    });
    form.setFieldsValue({ users: newUsers });
  };

  const saveForm = () => {
    const { userIds, ...formValues } = {
      ...form.getFieldsValue(),
      id: value.id,
    };
    UpdateDepartment(formValues);
  };

  return (
    <Form form={form} onValuesChange={onFinish}>
      <Form.Item name="title" label="中文名称" {...formItemLayout}>
        <Input />
      </Form.Item>
      <Form.Item name="titleEn" label="英文名称" {...formItemLayout}>
        <Input />
      </Form.Item>
      <Form.Item name="abbreviation" label="缩写" {...formItemLayout}>
        <Input />
      </Form.Item>
      <Form.Item name="userIds" label="人员" {...formItemLayout}>
        <Select
          mode="multiple"
          className="users-checkbox"
          options={optionUsers}
          onChange={addOrRemoveUsers}
          filterOption={(input, option) => option.label.indexOf(input) >= 0}
        />
      </Form.Item>
      <Form.List name="users">
        {(fields, { remove }) => {
          return fields.map((field, index) => (
            <Form.Item
              {...(index === 0 ? formItemLayout : formItemLayoutWithoutLabel)}
              {...field}
              label={index === 0 ? "详细" : null}
              className="form-item-narrow"
              key={field.key}
              validateTrigger={["onChange", "onBlur"]}
            >
              <DepartmentUserInput
                showLevel={false}
                showPosition={false}
                isFirst={index === 0}
                onRemove={() => remove(field.name)}
              />
            </Form.Item>
          ));
        }}
      </Form.List>

      <Form.Item {...formItemLayoutWithoutLabel}>
        <Button type="primary" onClick={saveForm}>
          保存
        </Button>
        <Button
          type="danger"
          onClick={() => props.onRemove && props.onRemove(value.id)}
        >
          删除此节点
        </Button>
      </Form.Item>
    </Form>
  );
};

export const DepartmentsPage = () => {
  const [departmentTree, setDepartmentTree] = useState();
  const [showCard, setShowCard] = useState(false);
  const [selectedNode, setSelectedNode] = useState();
  const [newDepartmentTitle, setNewDepartmentTitle] = useState("");
  const [newDepartmentFormVisible, setNewDepartmentFormVisible] =
    useState(false);

  const [users, setUsers] = useState([]);

  useEffect(() => {
    refreshTree();

    GetUsers((us) => {
      const users = us
        .sort((x, y) => (x.profiles.pinyin < y.profiles.pinyin ? -1 : 1))
        .map((u) => ({
          label: `${u.profiles.fullName || "佚名"} (${u.userName})`,
          value: u.id,
          option: {
            userName: u.userName,
            fullName: u.profiles.fullName || u.userName,
          },
        }));
      setUsers(users);
    });
  }, []);

  const refreshTree = () => {
    GetDepartments((data) => {
      loop(data, (item, index, array, parent) => {
        item.key = item.id;
        departmentMap[item.id] = item;
      });
      setDepartmentTree(data);
    });
  };

  //拖拽树节点
  const onTreeNodeDrop = (info) => {
    const dropKey = info.node.key;
    const dragKey = info.dragNode.key;
    const dropPos = info.node.pos.split("-");
    const dropPosition =
      info.dropPosition - Number(dropPos[dropPos.length - 1]);
    const data = [...departmentTree];

    // Find dragObject
    let dragObj = departmentMap[dragKey];
    let dropObj = departmentMap[dropKey];

    let arr = dragObj.parent ? dragObj.parent.children : data;
    arr.splice(
      arr.findIndex((x) => x === dragObj),
      1
    );

    if (!info.dropToGap) {
      // 扔在了节点上面
      dropObj.children = dropObj.children || [];
      dragObj.parent = dropObj;
      dropObj.children.push(dragObj);
    } else if (
      (info.node.children || []).length > 0 && // 包含子节点
      info.node.expanded && // 节点展开了（驻停一段时间，箭头朝下了）
      dropPosition === 1 // 放在了节点下面
    ) {
      // 设为节点下的一个子节点
      dropObj.children = dropObj.children || [];
      dragObj.parent = dropObj;
      dropObj.children.unshift(dragObj);
    } else {
      arr = dropObj.parent ? dropObj.parent.children : data;
      const index = arr.findIndex((x) => x === dropObj);
      dragObj.parent = dropObj.parent;
      if (dropPosition === -1) {
        // 节点上面，插到节点前面
        arr.splice(index, 0, dragObj);
      } else {
        // 节点下面，插到节点后面
        arr.splice(index + 1, 0, dragObj);
      }
    }

    setDepartmentTree(data);
  };
  const onTreeNodeSelect = (key) => {
    setShowCard(false);
    if (key.length === 1) {
      setSelectedNode(departmentMap[key]);
      GetDepartment(key, (data) => {
        setSelectedNode(data);
        setShowCard(true);
      });
    }
  };

  const onSaveStruct = () => {};
  const onAddSubmit = () => {
    const department = { title: newDepartmentTitle };
    AddDepartment(department, (data) => {
      if (data.id) {
        message.success("新增成功");
        refreshTree();
      }
    });
  };

  //暂存节点
  const saveNode = (id, values) => {};

  //删除节点
  const removeNode = (id) => {
    DeleteDepartment(id, () => {
      refreshTree();
      setNewDepartmentFormVisible(false);
    });
  };

  const menu = (
    <div className="ant-dropdown-menu" style={{ padding: 10 }}>
      <Input.Group compact>
        <Input
          placeholder="部门名称"
          value={newDepartmentTitle}
          onChange={(e) => setNewDepartmentTitle(e.target.value)}
        />
        <Button type="primary" onClick={onAddSubmit}>
          新建
        </Button>
      </Input.Group>
    </div>
  );

  return (
    <PageWrapper
      title="部门设置"
      extras={
        <Dropdown
          overlay={menu}
          placement="bottomRight"
          arrow
          visible={newDepartmentFormVisible}
          onVisibleChange={(v) => setNewDepartmentFormVisible(v)}
        >
          <Button>
            新建部门&nbsp;
            <FontAwesomeIcon icon="chevron-down" />
          </Button>
        </Dropdown>
      }
    >
      <Row>
        <Col span={8}>
          <Card
            title="部门"
            extra={<Button onClick={onSaveStruct}>保存结构</Button>}
          >
            {departmentTree && (
              <Tree
                className="draggable-tree"
                showIcon
                draggable
                blockNode
                defaultExpandAll
                onDrop={onTreeNodeDrop}
                onSelect={onTreeNodeSelect}
                treeData={departmentTree}
              ></Tree>
            )}
          </Card>
        </Col>
        <Col span={15} push={1}>
          {showCard ? (
            <Card title={selectedNode.title}>
              <NodeForm
                value={{ ...selectedNode }}
                optionUsers={users}
                onChange={saveNode}
                onRemove={removeNode}
              />
            </Card>
          ) : (
            ""
          )}
        </Col>
      </Row>
    </PageWrapper>
  );
};
