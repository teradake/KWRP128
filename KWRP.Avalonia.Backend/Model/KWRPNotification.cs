using KWRP.Avalonia.Backend.Enums;
using System;
using System.Collections.Generic;

namespace KWRP.Avalonia.Backend.Models
{
    public class KWRPNotification
    {
        public string? Message { get; init; }
        public NotifyMessageType MessageType { get; init; }
        public double Duration { get; init; }
        public string? CommandHeader { get; private set; } = null;
        public Action? Command { get; private set; } = null;

        private KWRPNotification()
        {
        }

        /// <summary>
        /// toast通知にコマンドを追加する
        /// </summary>
        /// <param name="header">ボタンの表示テキスト</param>
        /// <param name="command">ボタン押下時の処理</param>
        /// <returns></returns>
        public KWRPNotification WithCommand(string header, Action command)
        {
            this.CommandHeader = header;
            this.Command = command;
            return this;
        }

        /// <summary>
        /// toast通知するメッセージのインスタンスを返す
        /// </summary>
        /// <param name="message">通知メッセージ内容</param>
        /// <param name="type">メッセージタイプ(Info/Warn)</param>
        /// <param name="duration">メッセージが自然消滅するまでの時間(sec)。負の値の場合は消えない</param>
        /// <returns></returns>
        public static KWRPNotification Create(string message, NotifyMessageType type, double duration = -1)
        {
            return new KWRPNotification()
            {
                Message = message,
                MessageType = type,
                Duration = duration,
            };
        }
    }


    
}
