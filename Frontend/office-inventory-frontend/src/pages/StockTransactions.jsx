import React, { useEffect, useState } from "react";
import Select from "react-select";
import Layout from "../components/Layout";
import apiClient from "../api/apiClient";
import "../styles/stockTransactions.css";

const StockTransactions = () => {
  const [activeTab, setActiveTab] = useState("stockIn");
  const [items, setItems] = useState([]);
  const [formData, setFormData] = useState({
    itemID: "",
    quantity: "",
    notes: "",
  });
  const [loading, setLoading] = useState(false);

  // ✅ Fetch inventory items
  const fetchItems = async () => {
    try {
      setLoading(true);
      const { data } = await apiClient.get(
        "/api/InventoryItems/GetAllInventoryItems?pageNumber=1&pageSize=100"
      );
      if (data?.success && Array.isArray(data.data)) {
        setItems(
          data.data.map((item) => ({
            value: item.itemID,
            label: `${item.name} (${item.category})`,
          }))
        );
      }
    } catch (error) {
      console.error("Failed to load items:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems();
  }, []);

  // ✅ Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.itemID || !formData.quantity) {
      alert("Please select an item and enter a quantity.");
      return;
    }

    try {
      setLoading(true);
      const endpoint =
        activeTab === "stockIn"
          ? "/api/InventoryItems/Stock-in"
          : "/api/InventoryItems/Stock-out";

      const response = await apiClient.post(endpoint, formData);

      if (response.data?.success) {
        alert(
          activeTab === "stockIn"
            ? "Stock-In successful!"
            : "Stock-Out successful!"
        );
        setFormData({ itemID: "", quantity: "", notes: "" });
      } else {
        alert(response.data?.message || "Operation failed.");
      }
    } catch (error) {
      console.error("Error:", error);
      alert("An unexpected error occurred.");
    } finally {
      setLoading(false);
    }
  };

  // ✅ Handle tab change
  const handleTabChange = (tab) => {
    setActiveTab(tab);
    setFormData({ itemID: "", quantity: "", notes: "" });
  };

  return (
    <Layout>
      <div className="stock-container">
        <div className="header-section">
          <h2>Stock Transactions</h2>
        </div>

        {/* Tabs */}
        <div className="tabs">
          <button
            className={`tab-btn ${activeTab === "stockIn" ? "active" : ""}`}
            onClick={() => handleTabChange("stockIn")}
          >
            📦 Stock In
          </button>
          <button
            className={`tab-btn ${activeTab === "stockOut" ? "active" : ""}`}
            onClick={() => handleTabChange("stockOut")}
          >
            🚚 Stock Out
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="stock-form">
          <div className="form-row">
            <div className="form-group half-width">
              <label>Item</label>
              <Select
                classNamePrefix="react-select"
                options={items}
                value={items.find((opt) => opt.value === formData.itemID) || ""}
                onChange={(opt) =>
                  setFormData({ ...formData, itemID: opt ? opt.value : "" })
                }
                placeholder="Select item"
                isClearable
                isSearchable
                styles={{
                  control: (base) => ({
                    ...base,
                    minHeight: "30px",
                    fontSize: "0.8rem",
                    padding: "0 4px",
                  }),
                }}
              />
            </div>

            <div className="form-group half-width">
              <label>Quantity</label>
              <input
                type="number"
                min="1"
                value={formData.quantity}
                onChange={(e) =>
                  setFormData({ ...formData, quantity: e.target.value })
                }
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group full-width">
              <label>Notes (optional)</label>
              <input
                type="text"
                value={formData.notes}
                onChange={(e) =>
                  setFormData({ ...formData, notes: e.target.value })
                }
              />
            </div>
          </div>

          <button
            type="submit"
            className={`btn-primary small-btn ${
              activeTab === "stockOut" ? "btn-danger" : ""
            }`}
            disabled={loading}
          >
            {loading
              ? "Processing..."
              : activeTab === "stockIn"
              ? "➕ Stock In"
              : "➖ Stock Out"}
          </button>
        </form>
      </div>
    </Layout>
  );
};

export default StockTransactions;
