import { List } from "antd";

export const ModelSidebar = ({ tasks, activeTaskId, onChange }) => {
  return (
    <List size="large">
      {tasks.map((x) => (
        <List.Item
          key={x.id}
          onClick={() => onChange(x.id)}
          style={
            activeTaskId === x.id
              ? { background: "#f0f2f5", fontWeight: "bold", cursor: "pointer" }
              : { cursor: "pointer" }
          }
        >
          {x.modelName}
        </List.Item>
      ))}
    </List>
  );
};
