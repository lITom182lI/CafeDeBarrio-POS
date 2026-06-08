import { useState, useTransition } from "react";
import { Sidebar } from "./components/Sidebar";
import { ReportesYGraficos } from "./components/ReportesYGraficos";
import { MenuEInventario } from "./components/MenuEInventario";
import { Transacciones } from "./components/Transacciones";
import { GestionOperadores } from "./components/GestionOperadores";
import { Configuracion } from "./components/Configuracion";

export default function App() {
  const [activeTab, setActiveTab] = useState<string>("dashboard");
  const [operatorName] = useState<string>("Carlos Mendoza");
  const [, startTransition] = useTransition();

  const handleTabChange = (tab: string) => {
    startTransition(() => {
      setActiveTab(tab);
    });
  };

  const renderActiveView = () => {
    switch (activeTab) {
      case "dashboard":
        return <ReportesYGraficos />;
      case "productos":
        return <MenuEInventario />;
      case "transacciones":
        return <Transacciones />;
      case "operadores":
        return <GestionOperadores />;
      case "configuracion":
        return <Configuracion />;
      default:
        return <ReportesYGraficos />;
    }
  };

  return (
    <div className="app-layout">
      <Sidebar
        activeTab={activeTab}
        onChangeTab={handleTabChange}
        operatorName={operatorName}
      />
      <main className="main-content">
        <div className="page">{renderActiveView()}</div>
      </main>
    </div>
  );
}