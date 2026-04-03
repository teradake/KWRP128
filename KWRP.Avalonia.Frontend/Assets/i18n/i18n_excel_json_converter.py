from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Dict, List, Tuple

import pandas as pd


TARGET_SHEETS: List[str] = [
    "TopPage",
    "DxfPage",
    "RollerPage",
    "ConfigPage",
    "Canvas",
    "LaneEditor",
    "WorkAreaEditor",
    "ActivityEditor",
    "Domain2",
]

EXPECTED_COLS = ["Key", "ja", "en"]


def log(msg: str) -> None:
    """標準エラー出力へログ."""
    print(msg, file=sys.stderr)


def read_sheet(excel_path: Path, sheet_name: str) -> pd.DataFrame:
    """指定シートを DataFrame として読み込み、列チェック・空行処理を行う."""
    df = pd.read_excel(
        excel_path,
        sheet_name=sheet_name,
        engine="openpyxl",
        dtype=str,
        keep_default_na=False,  # NaN にしない（"" になる）
    )

    # 列名の前後空白対策
    df.columns = [str(c).strip() for c in df.columns]

    missing = [c for c in EXPECTED_COLS if c not in df.columns]
    if missing:
        raise ValueError(
            f"Sheet '{sheet_name}' に必要な列がありません: {missing}. "
            f"実際の列: {list(df.columns)}"
        )

    # 必要列のみ残す（余計な列があっても壊れない）
    df = df[EXPECTED_COLS].copy()

    # 値の正規化（前後空白除去）
    for c in EXPECTED_COLS:
        df[c] = df[c].astype(str).map(lambda s: s.strip())

    # 完全空行（Key/ja/en が全部空）を落とす
    all_empty = (df["Key"] == "") & (df["ja"] == "") & (df["en"] == "")
    df = df[~all_empty].copy()

    # Key が空なのに ja/en がある行はエラー
    bad = (df["Key"] == "") & ((df["ja"] != "") | (df["en"] != ""))
    if bad.any():
        bad_rows = df[bad].head(5)  # 全部出すと長いので先頭だけ
        raise ValueError(
            f"Sheet '{sheet_name}' に Key が空の不正行があります（ja/en に値あり）。例:\n"
            f"{bad_rows.to_string(index=False)}"
        )

    # Keyが空の行（つまり Key/ja/en 全部空でない場合は既に除外済み）を念のため排除
    df = df[df["Key"] != ""].copy()

    return df


def sanitize_value(s: str) -> str:
    """
    仕様：Excelの ja/en にダブルクォーテーションが含まれる場合、
    JSONが壊れる可能性があるためシングルクォーテーションに置換。
    ※ json.dump なら壊れませんが、仕様に従い置換を行う。
    """
    return s.replace('"', "'")


def build_language_maps(excel_path: Path) -> Tuple[Dict[str, str], Dict[str, str]]:
    """全シートを読み込み、ja/en の辞書へ統合（見つからないシートは警告してスキップ）."""
    xls = pd.ExcelFile(excel_path, engine="openpyxl")
    existing = set(xls.sheet_names)

    # 存在するシートだけ処理する
    sheets_to_process = [s for s in TARGET_SHEETS if s in existing]
    missing_sheets = [s for s in TARGET_SHEETS if s not in existing]

    if missing_sheets:
        log(
            "WARN: Excelに見つからない対象シートがあります。スキップして続行します: "
            + ", ".join(missing_sheets)
        )

    if not sheets_to_process:
        # ここは要件次第で「raise」か「空で続行」を選べます。
        # 今回は「報告して処理続行」を優先し、空の辞書を返します。
        log(
            "WARN: 対象シートが1つも見つかりませんでした。"
            " ja.json/en.json は空になります。"
            f"（Excelに存在するシート: {xls.sheet_names}）"
        )
        return {}, {}

    ja_map: Dict[str, str] = {}
    en_map: Dict[str, str] = {}

    for sheet in sheets_to_process:
        log(f"Reading sheet: {sheet}")
        df = read_sheet(excel_path, sheet)

        # 重複キー（シート内）
        if df["Key"].duplicated().any():
            dup_keys = df.loc[df["Key"].duplicated(), "Key"].unique().tolist()
            raise ValueError(f"Sheet '{sheet}' 内で Key が重複しています: {dup_keys}")

        for _, row in df.iterrows():
            key = row["Key"]
            ja_val = sanitize_value(row["ja"])
            en_val = sanitize_value(row["en"])

            # シート間の重複キー（distinct保証の保険）
            if key in ja_map or key in en_map:
                raise ValueError(f"Key がシート間で重複しています: '{key}'")

            ja_map[key] = ja_val
            en_map[key] = en_val

    return ja_map, en_map


def write_json(path: Path, data: Dict[str, str]) -> None:
    """JSONファイル出力."""
    with path.open("w", encoding="utf-8") as f:
        json.dump(
            data,
            f,
            ensure_ascii=False,  # 日本語を \uXXXX にしない
            indent=2,
            sort_keys=True,      # 差分が安定
        )
        f.write("\n")  # 末尾改行


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Convert i18n Excel sheets to ja.json and en.json"
    )
    parser.add_argument(
        "excel_path",
        type=str,
        help="Excel file path (e.g., i18n_phrases.xlsx)",
    )
    parser.add_argument(
        "--out-dir",
        type=str,
        default=".",
        help="Output directory (default: current directory)",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    excel_path = Path(args.excel_path)
    out_dir = Path(args.out_dir)

    if not excel_path.exists():
        raise FileNotFoundError(f"Excelファイルが見つかりません: {excel_path.resolve()}")

    if not out_dir.exists():
        raise FileNotFoundError(f"出力ディレクトリが見つかりません: {out_dir.resolve()}")

    log(f"Input: {excel_path.resolve()}")
    log(f"Output dir: {out_dir.resolve()}")

    ja_map, en_map = build_language_maps(excel_path)

    ja_out = out_dir / "ja.json"
    en_out = out_dir / "en.json"

    write_json(ja_out, ja_map)
    write_json(en_out, en_map)

    log(f"Wrote: {ja_out.resolve()}  (keys={len(ja_map)})")
    log(f"Wrote: {en_out.resolve()}  (keys={len(en_map)})")


if __name__ == "__main__":
    main()