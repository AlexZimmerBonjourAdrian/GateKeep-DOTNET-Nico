"use client"

import React, { useState } from 'react';
import Header from '../../../components/Header';

export default function listadoEventos() {

  return (
    <div className="container-nothing">
        <Header />

        <p>ladlaodadkkad</p>

        

        <style jsx>{`

          .container-nothing {
            margin: 0;
            width: 100%;
            height: 100%;
          }

          .filtros-container {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            align-items: center;
            padding: 12px;
            margin: 16px 24px;
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 1px 6px rgba(0,0,0,0.06);
            border: 1px solid rgba(0,0,0,0.06);
          }

          .filtros-container input[type="text"],
          .filtros-container input[type="date"] {
            height: 40px;
            padding: 8px 12px;
            border: 1px solid #e5e7eb;
            border-radius: 6px;
            background: #f8fafc;
            font-size: 14px;
            color: #111827;
            outline: none;
            transition: box-shadow 150ms ease, border-color 150ms ease, background 150ms ease;
          }

          .filtros-container input[type="text"] {
            min-width: 220px;
            flex: 1;
          }

          .filtros-container input[type="date"] {
            min-width: 150px;
          }

          .filtros-container input:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 4px rgba(37,99,235,0.08);
            background: #ffffff;
          }

          @media (max-width: 600px) {
            .filtros-container {
              padding: 10px;
              margin: 12px;
            }
            .filtros-container input[type="text"] {
              min-width: 100%;
              flex-basis: 100%;
            }
            .filtros-container input[type="date"] {
              min-width: calc(50% - 6px);
              flex: 1 1 48%;
            }
          }
        `}</style>
    </div>
  );
}

