import { Tabs, Upload, Button, List } from "antd";
import { useEffect, useState } from "react";
import { saveAs } from "file-saver";
import * as XLSX from "xlsx";
import { Flex } from "~/components/Flex";

export const TabLevel = ({ value, onChange }) => {
  const [items, setItems] = useState([]);

  const handleDownloadTemplate = () => {
    const ws = XLSX.utils.json_to_sheet(
      [
        ["任务编号", "任务名称", "步骤编号", "步骤名称"],
        ["备注：多个步骤的任务，从第二条步骤开始保持任务编号和任务名称列为空"],
      ],
      {
        skipHeader: true,
      }
    );
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Sheet1");
    const excelBuffer = XLSX.write(wb, {
      bookType: "xlsx",
      type: "array",
    });
    const blob = new Blob([excelBuffer], { type: "application/octet-stream" });
    saveAs(blob, `任务步骤模板.xlsx`);
  };
  useEffect(() => {
    setItems(
      value
        ? value.taskNames.map((x) => ({
            label: x.name,
            key: x.code,
            children: (
              <div
                style={{
                  maxHeight: "180px",
                  overflowY: "auto",
                  overflowX: "hidden",
                }}
              >
                <h3>流程</h3>
                <List
                  grid={{ gutter: 16, column: 4 }}
                  dataSource={value.taskSteps[x.code] || []}
                  renderItem={(item, i) => (
                    <List.Item key={item.code}>
                      {i + 1}. {item.name}
                    </List.Item>
                  )}
                ></List>
              </div>
            ),
          }))
        : []
    );
  }, [value]);
  return (
    <Flex direction="column" style={{ gap: 4 }}>
      <Tabs
        items={items}
        tabPosition="left"
        style={{ maxHeight: 200, width: "100%" }}
        tabBarGutter={0}
      ></Tabs>
      <Flex justify="space-between">
        <Upload
          accept=".xlsx"
          showUploadList={false}
          beforeUpload={(file) => {
            const reader = new FileReader();

            reader.onload = (e) => {
              const workbook = XLSX.read(e.target.result, {
                type: "buffer",
              });
              const sheetName = workbook.SheetNames[0];
              const sheet = workbook.Sheets[sheetName];
              const sheetData = XLSX.utils.sheet_to_txt(sheet);
              const records = sheetData.split("\n").map((x) => x.split("\t"));
              records.shift();
              const taskNames = [];
              const taskSteps = {};
              let lastTaskCode = undefined;
              records.forEach((r) => {
                if (r[0]) {
                  lastTaskCode = r[0];
                  const taskName = r[1];
                  taskNames.push({
                    code: lastTaskCode,
                    name: taskName,
                  });
                  taskSteps[lastTaskCode] = [];
                }
                taskSteps[lastTaskCode].push({
                  code: r[2],
                  name: r[3],
                });
              });
              onChange({ taskNames, taskSteps });
            };
            reader.readAsArrayBuffer(file);

            return false;
          }}
        >
          <Button>Excel 导入</Button>
        </Upload>
        <Button type="link" onClick={handleDownloadTemplate}>
          下载模板
        </Button>
      </Flex>
    </Flex>
  );
};
