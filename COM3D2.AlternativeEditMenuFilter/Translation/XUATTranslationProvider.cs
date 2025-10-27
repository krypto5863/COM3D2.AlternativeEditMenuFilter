using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//using XUnity.AutoTranslator.Plugin.Core;

namespace COM3D2.AlternativeEditMenuFilter.Translation.XUATProvider
{
    internal class XUATTranslationResult
    {
        private object translationResult;

        public bool Succeeded { get; set; }
        public string TranslatedText { get; set; }

        public XUATTranslationResult(object translationResult)
        {
            this.translationResult = translationResult;

            var type = translationResult.GetType();
            Succeeded = (bool)AccessTools.Property(type, "Succeeded").GetValue(translationResult, null);
            TranslatedText = (string)AccessTools.Property(type, "TranslatedText").GetValue(translationResult, null);
        }
    }

    internal class XUATTranslator
    {
        private static bool initialized = false;
        private static MethodInfo tryTranslateMethod;
        private static MethodInfo translateAsyncMethod;
        private static Type autoTranslatorType;
        private static PropertyInfo autoTranslatorDefault;

        public static bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            autoTranslatorType = AccessTools.TypeByName("XUnity.AutoTranslator.Plugin.Core.AutoTranslator");
            if (autoTranslatorType == null)
            {
                return false;
            }

            autoTranslatorDefault = autoTranslatorType.GetProperty("Default");

            var translatorInterface = autoTranslatorDefault.PropertyType;

            tryTranslateMethod = translatorInterface.GetMethod(
                "TryTranslate",
                new Type[] { typeof(string), typeof(string).MakeByRefType() }
            );

            var translationResultClass = AccessTools.TypeByName("XUnity.AutoTranslator.Plugin.Core.TranslationResult");
            var actionTClass = typeof(Action<>);
            var callbackClass = actionTClass.MakeGenericType(translationResultClass);

            translateAsyncMethod = translatorInterface.GetMethod(
                "TranslateAsync",
                new Type[] { typeof(string), callbackClass }
            );

            initialized = true;
            return true;
        }

        public static XUATTranslator Default
        {
            get
            {
                object translator = AccessTools.TypeByName("XUnity.AutoTranslator.Plugin.Core.AutoTranslator").GetProperty("Default").GetValue(null, null);
                return new XUATTranslator(translator);
            }
        }

        public bool TryTranslate(string text, out string translatedText)
        {
            var args = new object[]
            {
                text,
                null
            };

            var result = tryTranslateMethod.Invoke(
                translator,
                args);

            translatedText = args[1] as string;
            return (bool)result;
        }

        public void TranslateAsync(string text, Action<XUATTranslationResult> resolver)
        {
            Action<object> callback = o =>
            {
                resolver(new XUATTranslationResult(o));
            };

            translateAsyncMethod.Invoke(
                translator,
                new object[]
                {
                    text,
                    callback
                });
        }

        public XUATTranslator(object translator)
        {
            this.translator = translator;
        }

        private object translator;
    }

    public class XUATTranslationProvider : ITranslationProvider
    {
        private XUATTranslator translator;
        private readonly Queue<AsyncTResult> queue = new Queue<AsyncTResult>();

        public static ITranslationProvider Create()
        {
            if (XUATTranslator.Initialize())
            {
                return new XUATTranslationProvider();
            }

            return null;
        }

        protected XUATTranslationProvider()
        {
            translator = XUATTranslator.Default;
        }

        private class TResult : ITranslationResult
        {
            public string OriginalText
            {
                get; set;
            }

            public string TranslatedText
            {
                get; set;
            }

            public bool IsTranslationSuccessful
            {
                get;
                set;
            }

            public override string ToString()
            {
                return $"succeeded:{IsTranslationSuccessful}\n\ttext:{OriginalText}\n\ttranslated:{TranslatedText}";
            }
        }

        private class AsyncTResult : TResult, ITranslationAsyncResult
        {
            public bool IsReady { get; set; }

            public void Resolve(XUATTranslationResult r)
            {
                IsTranslationSuccessful = r.Succeeded;
                TranslatedText = r.TranslatedText;
                IsReady = true;
#if DEBUG
                LogVerbose($"AsyntTranslationResolved: {this}");
#endif
            }
        }

        public ITranslationResult Translate(string text)
        {
            string translatedText;
            bool translated = translator.TryTranslate(text, out translatedText);
            return new TResult
            {
                OriginalText = text,
                TranslatedText = translatedText,
                IsTranslationSuccessful = translated
            };
        }

        public ITranslationAsyncResult TranslateAsync(string text)
        {
            var result = new AsyncTResult
            {
                OriginalText = text,
                IsReady = false
            };
            translator.TranslateAsync(result.OriginalText, result.Resolve);
            //this.queue.Enqueue(result);
            return result;
        }

        public void ResetAsyncQueue()
        {
            //this.StopAllCoroutines();
            //this.queue.Clear();
            //this.StartCoroutine(this.StartTranslationCoroutine());
        }

        private IEnumerator StartTranslationCoroutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => queue.Count > 0);

                var r = queue.Dequeue();
                var complete = false;
#if DEBUG
                LogVerbose("Translating: {r.OriginalText}");
#endif
                translator.TranslateAsync(r.OriginalText, (t) =>
                {
                    complete = true;
                    r.Resolve(t);
                });

                yield return new WaitUntil(() => complete);
            }
        }

        private static void LogVerbose(object obj)
        {
#if DEBUG
            //Instance.Logger.LogInfo(obj);
#endif
        }
    }
}