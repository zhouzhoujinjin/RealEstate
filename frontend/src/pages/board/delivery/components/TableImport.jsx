import { Button, Form, Table, Upload } from "antd";
import { saveAs } from "file-saver";
import * as XLSX from "xlsx";
import { Flex } from "~/components/Flex";

export const TableImport = ({
  templateName,
  columns,
  rowKeyIndex,
  onChange,
  value,
  initialRows = []
}) => {
  const handleDownloadTemplate = () => {
    const ws = XLSX.utils.json_to_sheet(
      [columns, ...initialRows, ["备注：数字和时间需要首先键入“'”以防止Excel转义"]],
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
    saveAs(blob, `${templateName}.xlsx`);
  };

  return (
    <Table
      pagination={false}
      dataSource={value || []}
      rowKey={rowKeyIndex || 0}
      footer={() => (
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
                onChange(records);
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
      )}
    >
      {columns.map((x, i) => (
        <Table.Column key={x} title={x} dataIndex={i} />
      ))}
    </Table>
  );
};
