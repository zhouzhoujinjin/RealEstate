import { FinishedStat } from "./FinishedStat";
import styles from "./RightPanel.module.css";

export const RightPanel = ({ site }) => {
  return (
    <div className={styles.wrapper}>
      <FinishedStat site={site} />
    </div>
  );
};
