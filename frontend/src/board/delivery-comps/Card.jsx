import { alpha } from "@mantine/core";
import styles from "./card.module.less";
export const Card = ({ title, children, style }) => {
  const {height, ...rest} = style || {}
  return (
    <div
      className={styles.root}
      style={rest}
    >
      <div className={styles.head}>
        <h2 className={styles.title}>{title}</h2>
      </div>
      <div className={styles.body} style={{height: height}}>{children}</div>
    </div>
  );
};
