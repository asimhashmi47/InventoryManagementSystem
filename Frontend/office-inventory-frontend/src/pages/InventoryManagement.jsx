import React, { useEffect, useState } from "react";
import Select from "react-select";
import apiClient from "../api/apiClient";
import Layout from "../components/Layout";
import "../styles/inventoryManagement.css";

const InventoryManagement = () => {
  const [items, setItems] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    itemID: null,
    name: "",
    description: "",
    categoryID: "",
    quantity: "",
    unitPrice: "",
  });

  // ✅ Fetch categories
  const fetchCategories = async () => {
    try {
      const { data } = await apiClient.get("/api/InventoryItems/GetAllCategory");
      if (data?.success) {
        setCategories(
          data.data.map((c) => ({ value: c.categoryID, label: c.name }))
        );
      }
    } catch (error) {
      console.error("Failed to load categories:", error);
    }
  };

  // ✅ Fetch inventory items
  const fetchInventoryItems = async (searchTerm = "") => {
    try {
      setLoading(true);
      const endpoint = searchTerm
        ? `/api/InventoryItems/SearchInventoryItems?searchTerm=${searchTerm}&pageNumber=${page}&pageSize=10`
        : `/api/InventoryItems/GetAllInventoryItems?pageNumber=${page}&pageSize=10`;
      const { data } = await apiClient.get(endpoint);
      if (data?.success) setItems(data.data || []);
      else setItems([]);
    } catch (err) {
      setError("Failed to load inventory items");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
    fetchInventoryItems();
  }, [page]);

  // ✅ Handle form submission (Add / Update)
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (isEditing) {
        // 🔹 Update existing item
        const response = await apiClient.put(
          `/api/InventoryItems/UpdateInventoryItem`,
          formData
        );
        if (response.data?.success) {
          alert("Item updated successfully!");
          setIsEditing(false);
        } else {
          alert(response.data?.message || "Failed to update item.");
        }
      } else {
        // 🔹 Create new item
        const response = await apiClient.post(
          "/api/InventoryItems/CreateInventoryItem",
          formData
        );
        if (response.data?.success) {
          alert("Item created successfully!");
        } else {
          alert(response.data?.message || "Failed to create item.");
        }
      }

      fetchInventoryItems();
      setFormData({
        itemID: null,
        name: "",
        description: "",
        categoryID: "",
        quantity: "",
        unitPrice: "",
      });
    } catch (error) {
      alert("Error saving item.");
    }
  };

  // ✅ Search items
  const handleSearch = (e) => {
    e.preventDefault();
    fetchInventoryItems(search);
  };

  // ✅ Handle edit click
  const handleEdit = (item) => {
    setIsEditing(true);
    setFormData({
      itemID: item.itemID,
      name: item.name,
      description: item.description,
      categoryID: item.categoryID,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  // ✅ Cancel edit
  const cancelEdit = () => {
    setIsEditing(false);
    setFormData({
      itemID: null,
      name: "",
      description: "",
      categoryID: "",
      quantity: "",
      unitPrice: "",
    });
  };

  return (
    <Layout>
      <div className="inventory-container">
        <div className="header-section">
          <h2>Inventory Management</h2>
          <form onSubmit={handleSearch} className="search-bar">
            <input
              type="text"
              placeholder="Search items..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <button type="submit">🔍</button>
          </form>
        </div>

        {/* Form Section */}
        <form onSubmit={handleSubmit} className="inventory-form">
          <h4>{isEditing ? "Edit Item" : "Add New Item"}</h4>

          {/* Row 1 */}
          <div className="form-row">
            <div className="form-group half-width">
              <label>Name</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
                required
              />
            </div>

            <div className="form-group half-width">
              <label>Category</label>
              <Select
                classNamePrefix="react-select"
                options={categories}
                value={categories.find(
                  (opt) => opt.value === Number(formData.categoryID)
                )}
                onChange={(opt) =>
                  setFormData({ ...formData, categoryID: opt ? opt.value : "" })
                }
                placeholder="Select"
                isClearable
                isSearchable
                styles={{
                  control: (base) => ({
                    ...base,
                    minHeight: "28px",
                    fontSize: "0.8rem",
                    padding: "0 4px",
                  }),
                }}
              />
            </div>
          </div>

          {/* Row 2 */}
          <div className="form-row">
            <div className="form-group half-width">
              <label>Quantity</label>
              <input
                type="number"
                value={formData.quantity}
                onChange={(e) =>
                  setFormData({ ...formData, quantity: e.target.value })
                }
                required
              />
            </div>

            <div className="form-group half-width">
              <label>Unit Price</label>
              <input
                type="number"
                value={formData.unitPrice}
                onChange={(e) =>
                  setFormData({ ...formData, unitPrice: e.target.value })
                }
                required
              />
            </div>
          </div>

          {/* Row 3 */}
          <div className="form-row">
            <div className="form-group full-width">
              <label>Description</label>
              <input
                type="text"
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
              />
            </div>
          </div>

          <div className="form-actions">
            <button type="submit" className="btn-primary small-btn">
              {isEditing ? "✏️ Update Item" : "➕ Add Item"}
            </button>
            {isEditing && (
              <button
                type="button"
                className="btn-secondary small-btn"
                onClick={cancelEdit}
              >
                ✖ Cancel
              </button>
            )}
          </div>
        </form>

        {/* Table Section */}
        <div className="table-container">
          {loading ? (
            <p>Loading...</p>
          ) : error ? (
            <p className="error">{error}</p>
          ) : (
            <table className="inventory-table small">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Category</th>
                  <th>Quantity</th>
                  <th>Unit Price</th>
                  <th>Status</th>
                  <th>Edit</th>
                </tr>
              </thead>
              <tbody>
                {items.length > 0 ? (
                  items.map((item) => (
                    <tr key={item.itemID}>
                      <td>{item.itemID}</td>
                      <td>{item.name}</td>
                      <td>{item.category}</td>
                      <td>{item.quantity}</td>
                      <td className="text-right">{item.unitPrice}</td>
                      <td>{item.isActive ? "Active" : "Inactive"}</td>
                      <td>
                        <button
                          type="button"
                          className="btn-edit"
                          onClick={() => handleEdit(item)}
                        >
                          ✏️ Edit
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="7" className="no-data">
                      No items found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {/* Pagination */}
        <div className="pagination">
          <button
            disabled={page === 1}
            onClick={() => setPage((prev) => prev - 1)}
          >
            ◀ Prev
          </button>
          <span>Page {page}</span>
          <button onClick={() => setPage((prev) => prev + 1)}>Next ▶</button>
        </div>
      </div>
    </Layout>
  );
};

export default InventoryManagement;
