import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Stepper } from "@mantine/core";

export const ShotTurns = ({ items, current }) => {
  return (
    <Stepper
      active={current}
      ml={20}
      mr={20}
      mt={10}
      styles={{
        step: {
          flexDirection: "column",
          alignItems: "center",
        },
        separator: {
          marginTop: -25,
        },
        stepBody: {
          marginInlineStart: 0,
          marginTop: "var(--mantine-spacing-sm)",
        },
      }}
    >
      {items.map((x, i) => (
        <Stepper.Step
          label={x}
          key={x}
          color={i == current ? "green" : undefined}
          styles={{
            stepIcon: {
              background: i == current ? "var(--step-color)" : undefined,
            },
          }}
          completedIcon={<FontAwesomeIcon fixedWidth size="xl" icon="check" />}
          icon={
            <FontAwesomeIcon
              fixedWidth
              size="xl"
              icon={i == current ? "crosshairs" : "hourglass"}
            />
          }
        ></Stepper.Step>
      ))}
    </Stepper>
  );
};
