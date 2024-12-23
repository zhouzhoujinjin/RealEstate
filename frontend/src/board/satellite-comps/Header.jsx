import { Dispatch, SetStateAction, useEffect, useState } from "react";
import styles from "./Header.module.css";
import dayjs, { Dayjs } from "dayjs";
import { GetEnv, GetSystemStartTime } from "../services/satellite";

const getToday = (startTime, callback) => {
  const d = dayjs();
  const a = d.format("YYYY年MM月DD日 dddd");
  d.diff(startTime, "day");
  callback(a + " (已运行 " + d.format("D") + " 天)");
};

export const Header = () => {
  const [date, setDate] = useState("");
  const [title] = useState("试验运行数据可视化平台");
  const [startTime, setStartTime] = useState();
  const [temperature, setTemperature] = useState(null);
  const [humidity, setHumidity] = useState(null);
  useEffect(() => {
    GetSystemStartTime((d) => setStartTime(d));
  }, []);

  useEffect(() => {
    const id = setInterval(() => {
      GetEnv((data) => {
        setTemperature(data.tem.toFixed(2));
        setHumidity(data.hum.toFixed(2));
      });
    }, 1000 * 10);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    if (startTime) {
      const id = setInterval(() => getToday(startTime, setDate), 1000 * 60);
      getToday(startTime, setDate);
      return () => {
        clearInterval(id);
      };
    }
  }, [startTime]);

  return (
    <div className={styles.header}>
      <div className={styles.left}>{date}</div>
      <div className={styles.middle}>{title}</div>
      <div className={styles.right}>
        <span style={{ marginRight: 20 }}>
          试验室温度：{temperature || "--"}℃
        </span>
        <span>试验室湿度：{humidity || "--"}%</span>
      </div>
    </div>
  );
};
