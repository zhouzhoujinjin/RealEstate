import {
  IndexPage,
  ProfilePage,
  UsersPage,
  UserPage,
  RolesPage,
  RolePage,
  ConfigPage as SystemConfigPage,
  MenuPage,
  ValueSpacePage,
  ValueSpacesPage,
  DepartmentsPage,
} from "./pages";
import { FilesPage } from "./pages/task/FilesPage";
import { DatasetPage } from "./pages/task/DatasetPage";
import { ShareFoldersPage } from "./pages/task/ShareFoldersPage";
import { ShareScriptsPage } from "./pages/task/ShareScriptsPage";
import { TasksPage } from "./pages/task/TasksPage";
import { CheckRecordsPage } from "./pages/task/CheckRecordsPage";
import { ShotPage } from "./pages/board/delivery/ShotPage";
import { BoardsPage } from "./pages/board/delivery/BoardsPage";
import { SitesPage } from "./pages/site/SitesPage";
import { SettingPage } from "./pages/board/satellite/SettingPage";

export const COMPONENTS = {
  Index: IndexPage,
  Profile: ProfilePage,

  // 用户
  Users: UsersPage,
  User: UserPage,
  Roles: RolesPage,
  Role: RolePage,
  Departments: DepartmentsPage,

  // 系统
  SystemConfig: SystemConfigPage,
  Menu: MenuPage,
  ValueSpaces: ValueSpacesPage,
  ValueSpace: ValueSpacePage,

  Files: FilesPage,
  Tasks: TasksPage,
  ShareScripts: ShareScriptsPage,
  ShareFolders: ShareFoldersPage,
  Dataset: DatasetPage,
  CheckRecords: CheckRecordsPage,
  Sites: SitesPage,
  SatelliteBoardSetting: SettingPage,
  DeliveryBoards: BoardsPage,
  DeliveryShot: ShotPage,
};

export const paths = [
  { path: "/index", title: "首页", component: "Index" },
  {
    path: "/account/profile",
    exact: true,
    title: "个人设置",
    component: "Profile",
  },
  {
    path: "/departments",
    exact: true,
    title: "部门列表",
    component: "Departments",
  },
  {
    path: "/users/:id",
    title: "用户表单",
    component: "User",
  },
  {
    path: "/users",
    title: "用户列表",
    component: "Users",
  },
  {
    path: "/roles/:id",
    title: "角色表单",
    component: "Role",
  },
  {
    path: "/roles",
    title: "角色列表",
    component: "Roles",
  },
  {
    path: "/system/config",
    exact: true,
    title: "系统设置",
    component: "SystemConfig",
  },
  {
    path: "/system/menu",
    title: "菜单设置",
    component: "Menu",
  },
  {
    path: "/system/valueSpaces",
    exact: true,
    title: "字典列表",
    component: "ValueSpaces",
  },
  {
    path: "/system/valueSpaces/:name",
    exact: true,
    title: "字典表单",
    component: "ValueSpace",
  },
  {
    path: "/files",
    exact: true,
    title: "试验资源管理",
    component: "Files",
  },
  {
    path: "/shareScripts",
    exact: true,
    title: "测试用例管理",
    component: "ShareScripts",
  },
  {
    path: "/shareFolders",
    exact: true,
    title: "共享文件夹",
    component: "ShareFolders",
  },
  {
    path: "/dataset",
    exact: true,
    title: "试验数据管理",
    component: "Dataset",
  },
  {
    path: "/checkRecords",
    exact: true,
    title: "试验检查记录管理",
    component: "CheckRecords",
  },
  {
    path: "/sites",
    title: "ATP 工位管理",
    component: "Sites",
  },
  {
    path: "/satelliteBoardSetting",
    exact: true,
    title: "卫星看板设置",
    component: "SatelliteBoardSetting",
  },
  {
    path: "/deliveryBoards",
    exact: true,
    title: "运载看板管理",
    component: "DeliveryBoards",
  },
  {
    path: "/deliveryBoards/:id/shots",
    title: "运载看板发次设置",
    component: "DeliveryShot",
  },
];
