import { Table } from "@mantine/core";

export const BoardTable = ({ columns, elements, rowKey, fz = 20, mt = 0 }) => {
  const rows = elements.map((element, i) => (
    <Table.Tr key={element[rowKey || "name"] || i}>
      {columns.map((x) => (
        <Table.Td key={x.name}>
          {x.render ? x.render(element[x.name]) : element[x.name]}
        </Table.Td>
      ))}
    </Table.Tr>
  ));
  const tdColumns = columns.map((x) => (
    <Table.Th key={x.name}>{x.title}</Table.Th>
  ));
  return (
    <Table fz={fz} mt={mt}>
      {columns.filter((x) => x.title).length > 0 && (
        <Table.Thead>
          <Table.Tr>{tdColumns}</Table.Tr>
        </Table.Thead>
      )}
      <Table.Tbody>{rows}</Table.Tbody>
    </Table>
  );
};
