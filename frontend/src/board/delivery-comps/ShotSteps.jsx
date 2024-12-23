import { Timeline, Text } from "@mantine/core";
import { Card } from "./Card";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const ShotSteps = ({ items, current }) => {
  return (
    <Card title="当前测试流程" style={{ width: 400, height: "100%" }}>
      <Timeline mt={40} bulletSize={32} lineWidth={2} active={current}>
        {items.map((x, i) => (
          <Timeline.Item
            ml={10}
            mr={10}
            key={x.name}
            styles={{
              itemBullet: {
                background: x.finished ? "green" : undefined,
              },
            }}
            bullet={
              <FontAwesomeIcon
                size="lg"
                icon={
                  x.finished && x.endTime
                    ? "check"
                    : x.finished
                    ? "crosshairs"
                    : "hourglass"
                }
              />
            }
          >
            <Text
              fz={i == current ? 22 : 18}
              lh={i == current ? "22px" : "28px"}
              c={i == current ? "#fff" : undefined}
            >
              {x.name}
            </Text>
          </Timeline.Item>
        ))}
      </Timeline>
    </Card>
  );
};
