﻿using System.Collections.Generic;

namespace Utils
{
    public class Command
    {
        public string Id;
        public Dictionary<string, string> Param;
        public Command(string id, Dictionary<string, string> param = null)
        {
            Id = id;
            Param = param;
        }
        public override string ToString()
        {
            string str = Id;
            if (Param != null)
            {
                str += " : ";
                foreach (var p in Param)
                {
                    str += string.Format("{0}({1});", p.Key, p.Value);
                }
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
    public class Result
    {
        public string Id;
        public string Desc;
        public object Param;
        public Result(string id, string desc = null, object param = null)
        {
            Id = id;
            Desc = desc;
            Param = param;
        }
        public override string ToString()
        {
            string str = Id;
            if (Desc != null)
            {
                str += " : " + Desc;
            }
            return str;
        }
    }
    public class CommandHandler
    {
        protected Logger _log;
        public string Type;
        public string Name;
        public CommandHandler(object param)
        {
            Type = "CommandHandler";
            Name = param as string;
            AssignLogger();
        }
        protected void AssignLogger()
        {
            _log = new Logger(string.Format("{0}({1})", Type, Name));
        }
        private Command _currentCmd;
        virtual protected void PreExecute(Command cmd)
        {
            _currentCmd = cmd;
            _log.Info(_currentCmd.ToString());
        }
        virtual protected void PostExecute(Result result)
        {
            if (result.Id != "Ok")
            {
                _log.Warn(_currentCmd.ToString() + " " + result.ToString());
            }
            else if (!string.IsNullOrEmpty(result.Desc))
            {
                _log.Info(_currentCmd.ToString() + " " + result.ToString());
            }
        }
        virtual protected Result _execute(Command cmd)
        {
            return new Result("Ok");
        }
        virtual public Result Execute(Command cmd)
        {
            PreExecute(cmd);
            Result result = _execute(cmd);
            PostExecute(result);

            return result;
        }
        virtual public Result AsyncExecute(Command cmd)
        {
            PreExecute(cmd);
            Result result = new Result("Ok");
            PostExecute(result);

            return result;
        }
    }
}