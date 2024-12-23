import { useCallback, useEffect, useState } from "react";
import styles from "./LeftPanel.module.css";
import { ModelStat } from "./ModelStat";
import { SiteStat } from "./iteStat";
import { GetSites } from "../services/satellite";
import { modelStatData } from "../demo/data";

export const LeftPanel = ({ siteChange }) => {
  const [data, setData] = useState({
    handworks: modelStatData,
  });
  const [sites, setSites] = useState([]);
  const call = useCallback(() => {
    GetSites((d) => setSites(d));
  }, []);

  useEffect(() => {
    call();
  }, [call]);
  return (
    <div className={styles.wrapper}>
      <ModelStat stat={data.handworks} />
      <SiteStat locations={sites} siteChange={siteChange} />
    </div>
  );
};
