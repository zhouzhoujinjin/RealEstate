import { Cascader, Form, Input, Modal, Select } from "antd";

import { useEffect, useState } from "react";
import {
  GetAtpProjects,
  GetAllTestPoints,
  CreateSite,
  UpdateSite,
} from "~/services/task";
import { GetAtpOptions } from "~/services/utils";

const ipv4Pattern = /^(\d{1,3}\.){3}\d{1,3}$/;
export const SiteModal = ({ site, onClose }) => {
  const [testPoints, setTestPoints] = useState([]);
  const [atpOptions, setAtpOptions] = useState([]);
  const [atpIp, setAtpIp] = useState("");
  const [currentAtpProjectId, setCurrentAtpProjectId] = useState("");
  const [atpProjects, setAtpProjects] = useState([]);

  const [form] = Form.useForm();

  useEffect(() => {
    GetAtpOptions(setAtpOptions);
  }, []);

  useEffect(() => {
    if (site && atpOptions.length) {
      form.setFieldsValue(site);
      if (site.atpIp) {
        setAtpIp(site.atpIp);
      }
    }
  }, [site, atpOptions]);

  useEffect(() => {
    if (atpIp && currentAtpProjectId) {
      GetAllTestPoints(atpIp, currentAtpProjectId, (data) =>
        setTestPoints(data)
      );
    } else {
      setTestPoints([]);
    }
  }, [atpIp, currentAtpProjectId]);

  useEffect(() => {
    if (ipv4Pattern.test(atpIp)) {
      GetAtpProjects(atpIp, (data) => {
        data.forEach((x) => {
          x.key = x.id;
          x.value = x.id;
          x.label = x.name;
        });
        setAtpProjects(data);
        if (site.atpProjectId) {
          setCurrentAtpProjectId(site.atpProjectId);
        }
      });
    } else {
      setAtpProjects([]);
      setCurrentAtpProjectId("");
    }
  }, [atpIp]);

  return (
    <Modal
      title="ATP 工位"
      open
      onCancel={() => onClose()}
      onOk={() => {
        const values = form.getFieldsValue();
        values.atpProjectName = atpProjects.find(
          (x) => x.id == currentAtpProjectId
        )?.name;
        const atpTppIds = values.atpTppIds;
        values.atpTppUris = {};
        Object.keys(atpTppIds).forEach((key, i) => {
          let parent = testPoints
          atpTppIds[key].forEach((x) => {
            const current = parent.find((y) => y.value == x);
            if (current) {
              values.atpTppUris[key] = current.uri;
              parent = current.children;
            }
          });
        });

        console.log(values);
        if (site.id == -1) {
          CreateSite(values, onClose);
        } else {
          values.id = site.id;
          UpdateSite(values, onClose);
        }
      }}
    >
      <Form form={form} labelCol={{ span: 6 }}>
        <Form.Item name="siteCode" label="工位编号">
          <Input />
        </Form.Item>
        <Form.Item name="name" label="工位名称">
          <Input />
        </Form.Item>
        <Form.Item name="atpIp" label="ATP 服务 IP">
          <Input onChange={(e) => setAtpIp(e.target.value)} />
        </Form.Item>
        <Form.Item name="atpProjectId" label="ATP 项目">
          <Select
            options={atpProjects}
            onChange={(v) => {
              console.log(v);
              setCurrentAtpProjectId(v);
            }}
          />
        </Form.Item>
        {atpOptions.map((x) => (
          <Form.Item key={x.uriKey} name={["atpTppIds", x.uriKey]} label={x.uriName}>
            <Cascader options={testPoints} />
          </Form.Item>
        ))}
      </Form>
    </Modal>
  );
};
