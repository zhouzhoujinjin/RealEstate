import { Timeline, Text } from "@mantine/core";
import { Card } from "./Card";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const ShotProjects = ({ items, current }) => {
  return (
    <Card title="第一轮" style={{ width: 400, height: "100%" }}>
      <Timeline mt={40} bulletSize={40} lineWidth={2} active={current}>
        {items.map((x, i) => (
          <Timeline.Item
            bg={i == current ? "var(--mantine-primary-color-light)" : ""}
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
                icon={
                  x.finished && x.endTime
                    ? "check"
                    : i == current
                    ? "spinner"
                    : "hourglass"
                }
                spin={i == current}
              />
            }
          >
            <Text fz={22} lh="40px">
              {x.name}
            </Text>
          </Timeline.Item>
        ))}
      </Timeline>
    </Card>
  );
};
