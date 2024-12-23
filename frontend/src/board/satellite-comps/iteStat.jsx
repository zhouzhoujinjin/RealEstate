import styles from "./SiteStat.module.css";
import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import * as signalR from "@microsoft/signalr";

const SignalrUri = import.meta.env.VITE_SIGNALR_URI;

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${SignalrUri}/realtime`, {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets,
  })
  .configureLogging(signalR.LogLevel.Information)
  .build();

async function start() {
  try {
    await connection.start();
    console.log("SignalR Connected.");
  } catch (err) {
    console.log(err);
  }
}
start();

export const SiteStat = ({ locations, siteChange }) => {
  const [pause, setPause] = useState(0);
  const [sites, setSites] = useState([]);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const currentSite =
    selectedIndex >= 0 && selectedIndex < locations.length
      ? locations[selectedIndex]
      : {};

  useEffect(() => {
    setSites([...locations]);
    connection.on("currentChange", (siteCode, status, id) => {
      setSites((s) => {
        const index = s.findIndex((x) => x.siteCode == siteCode);
        if (index > -1) {
          const n = [...s];
          n[index].addonInfos.currentStatus = status;
          return n;
        }

        return s;
      });
    });
    return () => {
      connection.off("currentChange");
    };
  }, [locations]);

  useEffect(() => {
    const id = setInterval(() => {
      if (pause || !locations.length) return;
      setSelectedIndex((f) => (f + 1) % locations.length);
    }, 30 * 1000);

    return () => clearInterval(id);
  }, [locations, pause]);

  useEffect(() => {
    if (!pause) return;
    const id = setTimeout(() => {
      setPause(0);
    }, 60 * 1000);
    return () => clearTimeout(id);
  }, [pause]);

  useEffect(() => {
    currentSite.id && siteChange && siteChange(currentSite);
  }, [currentSite, siteChange]);

  return (
    <div className="card">
      <h4>实验室运行情况</h4>
      <div className={styles.legend}>
        <div className={styles.legendRow}>
          <div className={styles.legendItem}>
            <FontAwesomeIcon
              fixedWidth
              icon={["fas", "rectangle"]}
              color="#91CC75"
            />{" "}
            上电:{" "}
            {
              sites.filter(
                (x) =>
                  x.addonInfos.status == "Online" &&
                  x.addonInfos.currentStatus == "PowerOn"
              ).length
            }
            个
          </div>
          <div className={styles.legendItem}>
            <FontAwesomeIcon
              fixedWidth
              icon={["fas", "rectangle"]}
              color="#B03A5B"
            />{" "}
            断电:{" "}
            {
              sites.filter(
                (x) =>
                  x.addonInfos.status === "Online" &&
                  x.addonInfos.currentStatus === "PowerOff"
              ).length
            }
            个
          </div>
          <div className={styles.legendItem}>
            <FontAwesomeIcon
              fixedWidth
              icon={["fas", "rectangle"]}
              color="#ccc"
            />{" "}
            离线:{" "}
            {sites.filter((x) => x.addonInfos.status === "Offline").length}个
          </div>
        </div>
      </div>
      <div className={styles.wrapper}>
        {sites.map((y, i) => (
          <div
            className={`${styles.block} ${
              selectedIndex === i ? styles.active : ""
            }`}
            style={{
              backgroundColor:
                y.addonInfos.currentStatus === "PowerOn"
                  ? "#91CC75"
                  : y.addonInfos.currentStatus === "PowerOff" &&
                    y.addonInfos.status === "Online"
                  ? "#B03A5B"
                  : "#a0a0a0",
            }}
            key={y.siteCode}
            onClick={() => {
              setPause((p) => p++);
              setSelectedIndex(i);
            }}
          >
            {y.name}
          </div>
        ))}
      </div>
    </div>
  );
};
