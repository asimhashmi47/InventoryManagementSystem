import React, { useState, useMemo } from "react";
import Layout from "../components/Layout";
import "../styles/printStickers.css";
import QRCode from "react-qr-code";

const PrintStickers = () => {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");

  const username = "Asim Aziz";
  const dateTime = useMemo(() => new Date().toLocaleString(), []);

  const handlePrint = () => window.print();

  // ✅ Each field on a separate line in QR data
  const qrData = `Code: ${code || "-"}\nName: ${name || "-"}\nUser: ${username}\nDateTime: ${dateTime}`;

  return (
    <Layout>
      <div className="print-container">

        {/* Left side - Form section */}
        <div className="form-section">
          <h4 className="mb-4 text-muted">Sticker Details</h4>

          <div className="form-group mb-3">
            <label>Product Code</label>
            <input
              type="text"
              className="form-control"
              placeholder="Enter product code"
              value={code}
              onChange={(e) => setCode(e.target.value)}
            />
          </div>

          <div className="form-group mb-4">
            <label>Product Name</label>
            <input
              type="text"
              className="form-control"
              placeholder="Enter product name"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </div>

          <button className="btn btn-primary mt-3" onClick={handlePrint}>
            🖨️ Print Sticker
          </button>
        </div>

        {/* Right side - Sticker preview */}
        <div className="preview-section margin-left-3">
          <div className="sticker-preview square" id="print-area">

            {/* ✅ QR Code top center */}
            <div className="qr-container">
              <QRCode value={qrData} size={70} className="qr-preview" />
            </div>

            <div className="sticker-row">
              <span className="label">Code:</span>
              <span className="value underline">{code || " "}</span>
            </div>

            <div className="sticker-row">
              <span className="label">Name:</span>
              <span className="value underline">{name || " "}</span>
            </div>

            <div className="footer-full">
              {username} | {dateTime}
            </div>

          </div>
        </div>
      </div>
    </Layout>
  );
};

export default PrintStickers;
