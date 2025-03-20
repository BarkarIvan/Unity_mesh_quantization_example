# 🛠 Mesh Optimizer – Пример квантования мешей

Этот инструмент для Unity **сжимает меши**, уменьшая их размер.

##  Что делает?
### 🔹 **Квантизация позиций (SNORM16, 4 канала)**
- Использует **множитель**.

### 🔹 **Квантизация нормалей и тангентов (SNORM16, 4D-кватернион)**
- **Нормали и тангенты кодируются в кватернион (`qTangent`)**.

### 🔹 **Сохранение меша как нового `.asset`**
- Оптимизированный меш сохраняется в **той же папке** с `_Optimized.asset`.

---

##  **Форматы хранения**
| **Компонент**     | **Формат**      | **Описание** |
|------------------|---------------|-------------|
| **Позиции**     | `SNORM16 x4`   | XYZ + масштаб |
| **Нормали и тангенты** | `SNORM16 x4` | Кватернион |
| **UV**          | `FP16 x2`      | Половинная точность |



##  **Как использовать?**
1. Открой **Tools → Mesh Optimizer** в Unity.
2. Выбери **меш**.
3. Нажми **"Optimize & Save"** – новый `.asset` появится рядом с оригиналом.

---

# 🛠 Mesh quantization example

This Unity tool **compresses meshes**, reducing memory usage

##  What does it do?
### 🔹 **Position Quantization (SNORM16, 4 channels)**


### 🔹 **Normal & Tangent Quantization (SNORM16, 4D Quaternion)**
- **Normals and tangents are packed into a quaternion (`qTangent`)** for compact storage.


### 🔹 **Saves as a new `.asset`**
- The optimized mesh is stored **in the same folder** with `_Optimized.asset`.

---

##  **Data Formats**
| **Component**     | **Format**      | **Description** |
|------------------|---------------|-------------|
| **Positions**     | `SNORM16 x4`   | XYZ + scale |
| **Normals & Tangents** | `SNORM16 x4` | Quaternion |
| **UV**          | `FP16 x2`      | Half precision |




##  **How to Use?**
1. Open **Tools → Mesh Optimizer** in Unity.
2. Select **a mesh**.
3. Click **"Optimize & Save"** – the new `.asset` appears next to the original.
