import ReactDOM from "react-dom/client";

import dayjs from "dayjs";
import zhCN from "dayjs/locale/zh-cn";
import "@mantine/core/styles.css";
dayjs.locale(zhCN);

import styles from "./satelliteBoardPage.module.css";
import { Header } from "./satellite-comps/Header";
import { LeftPanel } from "./satellite-comps/LeftPanel";
import { RightPanel } from "./satellite-comps/RightPanel";
import { MantineProvider } from "@mantine/core";

import { config } from "@fortawesome/fontawesome-svg-core";
import { useEffect, useState } from "react";

config.styleDefault = "light";

export const SatelliteBoardPage = () => {
  const [site, setSite] = useState();
  return (
    <>
      <Header />
      <div className={styles.main}>
        <LeftPanel siteChange={(s) => setSite(s)} />
        <MantineProvider forceColorScheme="dark">
          <RightPanel site={site} />
        </MantineProvider>
      </div>
    </>
  );
};

ReactDOM.createRoot(document.getElementById("root")).render(
  <SatelliteBoardPage />
);
